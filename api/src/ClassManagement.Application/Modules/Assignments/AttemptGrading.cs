using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;

// Shared finalize-and-grade routine used by both the student SubmitAttempt handler and the system
// lifecycle (auto-submit) handler, so the two paths can never diverge. Given a tracked attempt (with
// its answers loaded), it: ensures an answer row exists for every snapshot question, runs auto-grading
// over the snapshot's correct options, writes per-answer auto scores, and sets the attempt's totals +
// next status (AutoGraded when only objective questions, NeedManualGrading when any writing question).
// Does NOT commit — the caller owns the SaveChanges so the whole finalize is one transaction.
internal static class AttemptGrading
{
    public static async Task FinalizeAsync(
        IUnitOfWork unitOfWork,
        IAutoGradingService grader,
        Attempt attempt,
        DateTime now,
        bool autoSubmitted,
        CancellationToken ct
    )
    {
        // Resolve the assignment's snapshot.
        var snapshotRepo = unitOfWork.Repository<AssignmentSnapshot>();
        var snapshotId = (
            await snapshotRepo.ToListAsync(
                snapshotRepo
                    .Query()
                    .Where(s => s.AssignmentId == attempt.AssignmentId)
                    .Select(s => s.Id)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (snapshotId == 0)
            throw new BadException("Assignment.SnapshotMissing");

        var sqRepo = unitOfWork.Repository<SnapshotQuestion>();
        var snapshotQuestions = await sqRepo.ToListAsync(
            sqRepo
                .Query()
                .Where(q => q.SnapshotId == snapshotId)
                .Select(q => new SnapshotQuestionRef(q.Id, q.Type, q.Point)),
            ct
        );

        var snapshotQuestionIds = snapshotQuestions.Select(q => q.Id).ToList();

        var optRepo = unitOfWork.Repository<SnapshotOption>();
        var correctRows = await optRepo.ToListAsync(
            optRepo
                .Query()
                .Where(o => snapshotQuestionIds.Contains(o.SnapshotQuestionId) && o.IsCorrect)
                .Select(o => new CorrectRef(o.SnapshotQuestionId, o.Id)),
            ct
        );
        var correctByQuestion = correctRows
            .GroupBy(r => r.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<long>)g.Select(r => r.OptionId).ToList());

        // Ensure one answer row per snapshot question (unanswered objective questions score 0).
        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);
        foreach (var sq in snapshotQuestions)
        {
            if (!answerByQuestion.ContainsKey(sq.Id))
            {
                var created = new AttemptAnswer { SnapshotQuestionId = sq.Id, LastSavedAt = now };
                attempt.Answers.Add(created);
                answerByQuestion[sq.Id] = created;
            }
        }

        var gradingQuestions = snapshotQuestions
            .Select(sq =>
            {
                var answer = answerByQuestion[sq.Id];
                var correct = correctByQuestion.TryGetValue(sq.Id, out var c) ? c : [];
                return new GradingQuestion(
                    sq.Id,
                    sq.Type,
                    sq.Point,
                    correct,
                    answer.SelectedOptionIds,
                    !string.IsNullOrWhiteSpace(answer.TextAnswer)
                );
            })
            .ToList();

        var result = grader.Grade(gradingQuestions);

        var scoreByQuestion = result.Answers.ToDictionary(a => a.SnapshotQuestionId);
        foreach (var (questionId, answer) in answerByQuestion)
        {
            if (scoreByQuestion.TryGetValue(questionId, out var graded))
            {
                answer.AutoScore = graded.AutoScore;
                answer.IsAutoGraded = graded.IsAutoGraded;
            }
            answer.SubmittedAt = now;
        }

        attempt.SubmittedAt = now;
        attempt.AutoSubmitted = autoSubmitted;
        attempt.TotalAutoScore = result.TotalAutoScore;

        if (result.HasManualPending)
        {
            attempt.Status = AttemptStatus.NeedManualGrading;
            attempt.TotalScore = null; // completed after MVP-6 manual grading
        }
        else
        {
            attempt.Status = AttemptStatus.AutoGraded;
            attempt.TotalScore = result.TotalAutoScore;
        }

        unitOfWork.Attempts.Update(attempt);
    }

    private readonly record struct SnapshotQuestionRef(long Id, QuestionType Type, decimal Point);

    private readonly record struct CorrectRef(long SnapshotQuestionId, long OptionId);
}

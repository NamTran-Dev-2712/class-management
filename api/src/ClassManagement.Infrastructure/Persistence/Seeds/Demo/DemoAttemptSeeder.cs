using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Approved students attempt each published assignment. Objective answers are auto-graded with the real
// IAutoGradingService (the finalize mirrors the internal AttemptGrading routine — it's internal to the
// Application assembly so it can't be called from here). Mixed (Manual-policy) assignments then get
// manual grades on their writing questions, flipping attempts to Graded and releasing the grades —
// except the last attempt per such assignment, left NeedManualGrading to populate the grading queue.
internal static class DemoAttemptSeeder
{
    private const decimal ManualScoreRatio = 0.8m;

    public static async Task SeedAsync(
        ApplicationDbContext context,
        IAutoGradingService grader,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;
        var attempts = new List<Attempt>();

        var published = state
            .Assignments.Where(a =>
                a.Snapshot is not null
                && (a.Status == AssignmentStatus.Open || a.Status == AssignmentStatus.Closed)
            )
            .ToList();

        foreach (var assignment in published)
        {
            var @class = state.Classes.FirstOrDefault(c => c.Id == assignment.ClassId);
            if (@class is null)
                continue;

            var approved = @class
                .Memberships.Where(m => m.Status == MembershipStatus.Approved)
                .Select(m => m.StudentId)
                .Take(5)
                .ToList();

            var snapshotQuestions = assignment
                .Snapshot!.Questions.OrderBy(q => q.DisplayOrder)
                .ToList();

            var index = 0;
            foreach (var studentId in approved)
            {
                var answersWell = index % 3 != 0; // ~2/3 of students answer well
                var startedAt =
                    assignment.Status == AssignmentStatus.Open
                        ? now.AddDays(-1)
                        : (assignment.OpensAt ?? now.AddDays(-9)).AddHours(2);
                var submittedAt = startedAt.AddMinutes(10);

                var attempt = new Attempt
                {
                    AssignmentId = assignment.Id,
                    StudentId = studentId,
                    AttemptNumber = 1,
                    StartedAt = startedAt,
                    DeadlineAt = startedAt.AddMinutes(assignment.TimeLimitMinutes ?? 30),
                    QuestionOrder = snapshotQuestions.Select(q => q.Id).ToList(),
                    IpAddress = "127.0.0.1",
                    UserAgent = "DemoSeeder",
                    CreatedAt = startedAt,
                    UpdatedAt = startedAt,
                };

                foreach (var sq in snapshotQuestions)
                {
                    var answer = new AttemptAnswer
                    {
                        SnapshotQuestionId = sq.Id,
                        LastSavedAt = startedAt,
                        CreatedAt = startedAt,
                    };

                    if (IsChoice(sq.Type))
                    {
                        var correct = sq.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                        var firstOption = sq
                            .Options.OrderBy(o => o.DisplayOrder)
                            .Select(o => o.Id)
                            .Take(1)
                            .ToList();
                        answer.SelectedOptionIds = answersWell ? correct : firstOption;
                    }
                    else
                    {
                        answer.TextAnswer = answersWell
                            ? "Bài làm tự luận mẫu: trình bày đầy đủ các bước và nêu kết luận."
                            : "Em chưa làm kịp phần này.";
                    }

                    attempt.Answers.Add(answer);
                }

                Finalize(grader, attempt, snapshotQuestions, submittedAt);
                attempts.Add(attempt);
                index++;
            }
        }

        await context.Attempts.AddRangeAsync(attempts);
        await context.SaveChangesAsync();

        // Manual grading pass for Manual-policy assignments (their writing questions), then release.
        var manualGrades = new List<ManualGrade>();
        foreach (
            var assignment in published.Where(a =>
                a.GradePublishPolicy == GradePublishPolicy.Manual
            )
        )
        {
            var writing = assignment.Snapshot!.Questions.Where(q => IsWriting(q.Type)).ToList();
            if (writing.Count == 0)
                continue;

            var assignmentAttempts = attempts.Where(a => a.AssignmentId == assignment.Id).ToList();
            for (var i = 0; i < assignmentAttempts.Count; i++)
            {
                // Leave the last attempt ungraded so the teacher's grading queue isn't empty.
                if (i == assignmentAttempts.Count - 1)
                    continue;

                var attempt = assignmentAttempts[i];
                var totalManual = 0m;
                foreach (var wq in writing)
                {
                    var score = Math.Round(wq.Point * ManualScoreRatio, 2);
                    totalManual += score;
                    manualGrades.Add(
                        new ManualGrade
                        {
                            AttemptId = attempt.Id,
                            SnapshotQuestionId = wq.Id,
                            Score = score,
                            Feedback = "Trình bày tốt; cần nêu rõ kết luận hơn.",
                            CreatedAt = now,
                            UpdatedAt = now,
                        }
                    );
                }

                attempt.TotalManualScore = totalManual;
                attempt.Status = AttemptStatus.Graded;
                attempt.TotalScore = (attempt.TotalAutoScore ?? 0m) + totalManual;
            }

            assignment.GradesReleasedAt = now;
        }

        await context.ManualGrades.AddRangeAsync(manualGrades);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded {AttemptCount} demo attempts + {GradeCount} manual grades",
            attempts.Count,
            manualGrades.Count
        );
    }

    // Mirrors the internal AttemptGrading.FinalizeAsync (auto path): grade objective answers, set totals
    // and next status (AutoGraded when objective-only, NeedManualGrading when any writing question).
    private static void Finalize(
        IAutoGradingService grader,
        Attempt attempt,
        List<SnapshotQuestion> snapshotQuestions,
        DateTime submittedAt
    )
    {
        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);

        var gradingQuestions = snapshotQuestions
            .Select(sq =>
            {
                var answer = answerByQuestion[sq.Id];
                var correct = sq.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
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

        foreach (var answer in attempt.Answers)
        {
            if (scoreByQuestion.TryGetValue(answer.SnapshotQuestionId, out var graded))
            {
                answer.AutoScore = graded.AutoScore;
                answer.IsAutoGraded = graded.IsAutoGraded;
            }
            answer.SubmittedAt = submittedAt;
        }

        attempt.SubmittedAt = submittedAt;
        attempt.AutoSubmitted = false;
        attempt.TotalAutoScore = result.TotalAutoScore;

        if (result.HasManualPending)
        {
            attempt.Status = AttemptStatus.NeedManualGrading;
            attempt.TotalScore = null;
        }
        else
        {
            attempt.Status = AttemptStatus.AutoGraded;
            attempt.TotalScore = result.TotalAutoScore;
        }
    }

    private static bool IsChoice(QuestionType type) =>
        type is QuestionType.SingleChoice or QuestionType.MultipleChoice or QuestionType.TrueFalse;

    private static bool IsWriting(QuestionType type) =>
        type is QuestionType.ShortWriting or QuestionType.LongWriting;
}

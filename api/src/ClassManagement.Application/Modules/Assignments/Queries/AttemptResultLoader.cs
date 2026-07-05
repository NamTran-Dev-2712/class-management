using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;

// Builds an attempt result DTO. Score visibility honours the assignment's grade-publish policy (the
// teacher view forces release). The per-question breakdown — including teacher feedback and, when the
// assignment allows it, the correct options — is included only when released.
internal static class AttemptResultLoader
{
    public static async Task<AttemptResultDto> BuildAsync(
        IUnitOfWork unitOfWork,
        Attempt attempt,
        bool forceReleased,
        CancellationToken ct
    )
    {
        var avRepo = unitOfWork.Repository<AssignmentView>();
        var info = (
            await avRepo.ToListAsync(
                avRepo
                    .Query()
                    .Where(a => a.Id == attempt.AssignmentId)
                    .Select(a => new AssignmentInfo(
                        a.PublicId,
                        a.Title,
                        a.TotalPoint,
                        a.Status,
                        a.GradePublishPolicy,
                        a.ClosesAt,
                        a.GradesReleasedAt,
                        a.ShowAnswersAfterGrade
                    ))
                    .Take(1),
                ct
            )
        ).First();

        var released = forceReleased || IsReleased(info, attempt.Status);
        var showAnswers = released && info.ShowAnswersAfterGrade;

        IReadOnlyList<AttemptAnswerResultDto> answers = [];
        if (released)
            answers = await BuildAnswersAsync(unitOfWork, attempt, showAnswers, ct);

        return new AttemptResultDto
        {
            PublicId = attempt.PublicId,
            AssignmentPublicId = info.PublicId,
            AssignmentTitle = info.Title,
            Status = attempt.Status.ToString(),
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            AutoSubmitted = attempt.AutoSubmitted,
            ViolationCount = attempt.ViolationCount,
            IsFlagged = attempt.IsFlagged,
            TotalPoint = info.TotalPoint,
            ScoreReleased = released,
            ShowAnswers = showAnswers,
            TotalAutoScore = released ? attempt.TotalAutoScore : null,
            TotalManualScore = released ? attempt.TotalManualScore : null,
            TotalScore = released ? attempt.TotalScore : null,
            Answers = answers,
        };
    }

    private static bool IsReleased(AssignmentInfo info, AttemptStatus attemptStatus)
    {
        // No result until the attempt is submitted.
        if (attemptStatus == AttemptStatus.InProgress)
            return false;

        return info.GradePublishPolicy switch
        {
            nameof(GradePublishPolicy.Immediate) => true,
            nameof(GradePublishPolicy.AfterDeadline) => info.Status
                == nameof(AssignmentStatus.Closed)
                || (info.ClosesAt is DateTime closes && closes <= DateTime.UtcNow),
            nameof(GradePublishPolicy.Manual) => info.GradesReleasedAt is not null,
            _ => false,
        };
    }

    private static async Task<IReadOnlyList<AttemptAnswerResultDto>> BuildAnswersAsync(
        IUnitOfWork unitOfWork,
        Attempt attempt,
        bool showAnswers,
        CancellationToken ct
    )
    {
        var questionIds = attempt.QuestionOrder;
        var sqRepo = unitOfWork.Repository<SnapshotQuestion>();
        var questions = (
            await sqRepo.ToListAsync(
                sqRepo
                    .Query()
                    .Where(q => questionIds.Contains(q.Id))
                    .Select(q => new QuestionRow(q.Id, q.PublicId, q.Type, q.Content, q.Point)),
                ct
            )
        ).ToDictionary(q => q.Id);

        // Options for the objective questions (for selected/correct flags).
        var soRepo = unitOfWork.Repository<SnapshotOption>();
        var options = await soRepo.ToListAsync(
            soRepo
                .Query()
                .Where(o => questionIds.Contains(o.SnapshotQuestionId))
                .OrderBy(o => o.SnapshotQuestionId)
                .ThenBy(o => o.DisplayOrder)
                .Select(o => new OptionRow(
                    o.SnapshotQuestionId,
                    o.Id,
                    o.PublicId,
                    o.Content,
                    o.IsCorrect
                )),
            ct
        );
        var optionsByQuestion = options
            .GroupBy(o => o.SnapshotQuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);
        var grades = await unitOfWork.ManualGrades.GetByAttemptAsync(attempt.Id, ct);
        var gradeByQuestion = grades.ToDictionary(g => g.SnapshotQuestionId);

        var results = new List<AttemptAnswerResultDto>();
        var position = 1;
        foreach (var qId in questionIds)
        {
            if (!questions.TryGetValue(qId, out var q))
            {
                position++;
                continue;
            }

            var isWriting =
                q.Type == QuestionType.ShortWriting || q.Type == QuestionType.LongWriting;
            answerByQuestion.TryGetValue(qId, out var answer);
            gradeByQuestion.TryGetValue(qId, out var grade);
            var selected = answer?.SelectedOptionIds ?? [];

            IReadOnlyList<AttemptAnswerOptionDto> opts = isWriting
                ? []
                :
                [
                    .. (optionsByQuestion.TryGetValue(qId, out var list) ? list : []).Select(
                        o => new AttemptAnswerOptionDto
                        {
                            PublicId = o.PublicId,
                            Content = o.Content,
                            IsSelected = selected.Contains(o.Id),
                            IsCorrect = showAnswers ? o.IsCorrect : null,
                        }
                    ),
                ];

            results.Add(
                new AttemptAnswerResultDto
                {
                    QuestionPublicId = q.PublicId,
                    DisplayPosition = position++,
                    Type = q.Type.ToString(),
                    Content = q.Content,
                    Point = q.Point,
                    IsWriting = isWriting,
                    AutoScore = answer?.AutoScore,
                    IsAutoGraded = answer?.IsAutoGraded ?? false,
                    TextAnswer = isWriting ? answer?.TextAnswer : null,
                    ManualScore = isWriting ? grade?.Score : null,
                    Feedback = isWriting ? grade?.Feedback : null,
                    Options = opts,
                }
            );
        }

        return results;
    }

    private readonly record struct AssignmentInfo(
        Guid PublicId,
        string Title,
        decimal? TotalPoint,
        string Status,
        string GradePublishPolicy,
        DateTime? ClosesAt,
        DateTime? GradesReleasedAt,
        bool ShowAnswersAfterGrade
    );

    private readonly record struct QuestionRow(
        long Id,
        Guid PublicId,
        QuestionType Type,
        string Content,
        decimal Point
    );

    private readonly record struct OptionRow(
        long SnapshotQuestionId,
        long Id,
        Guid PublicId,
        string Content,
        bool IsCorrect
    );
}

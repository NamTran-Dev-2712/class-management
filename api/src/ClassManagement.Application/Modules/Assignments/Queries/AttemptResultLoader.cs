using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Builds an attempt result DTO. Score visibility honours the assignment's grade-publish policy (the
// teacher view forces release). The per-question breakdown is included only when released.
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
                        a.GradesReleasedAt
                    ))
                    .Take(1),
                ct
            )
        ).First();

        var released = forceReleased || IsReleased(info, attempt.Status);

        IReadOnlyList<AttemptAnswerResultDto> answers = [];
        if (released)
            answers = await BuildAnswersAsync(unitOfWork, attempt, ct);

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
            TotalPoint = info.TotalPoint,
            ScoreReleased = released,
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
                    .Select(q => new QuestionRow(q.Id, q.PublicId, q.Type.ToString(), q.Point)),
                ct
            )
        ).ToDictionary(q => q.Id);

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.SnapshotQuestionId);

        var results = new List<AttemptAnswerResultDto>();
        var position = 1;
        foreach (var qId in questionIds)
        {
            if (!questions.TryGetValue(qId, out var q))
            {
                position++;
                continue;
            }

            answerByQuestion.TryGetValue(qId, out var answer);
            results.Add(
                new AttemptAnswerResultDto
                {
                    QuestionPublicId = q.PublicId,
                    DisplayPosition = position++,
                    Type = q.Type,
                    Point = q.Point,
                    AutoScore = answer?.AutoScore,
                    IsAutoGraded = answer?.IsAutoGraded ?? false,
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
        DateTime? GradesReleasedAt
    );

    private readonly record struct QuestionRow(long Id, Guid PublicId, string Type, decimal Point);
}

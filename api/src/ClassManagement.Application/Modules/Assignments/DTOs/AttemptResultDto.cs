namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// An attempt's result. Score visibility honours the assignment's grade-publish policy: when grades
/// aren't released yet, <see cref="ScoreReleased"/> is false and the score fields are null (the FE
/// shows "pending"). Per-question breakdown is included only when released.
/// </summary>
public sealed record AttemptResultDto
{
    public Guid PublicId { get; init; }
    public Guid AssignmentPublicId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public bool AutoSubmitted { get; init; }

    public decimal? TotalPoint { get; init; }
    public bool ScoreReleased { get; init; }
    public decimal? TotalAutoScore { get; init; }
    public decimal? TotalManualScore { get; init; }
    public decimal? TotalScore { get; init; }

    public IReadOnlyList<AttemptAnswerResultDto> Answers { get; init; } = [];
}

/// <summary>Per-question result (populated only when the score is released).</summary>
public sealed record AttemptAnswerResultDto
{
    public Guid QuestionPublicId { get; init; }
    public int DisplayPosition { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public decimal? AutoScore { get; init; }
    public bool IsAutoGraded { get; init; }
}

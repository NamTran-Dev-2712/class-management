namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// The live test-taking session for one attempt: the per-attempt shuffled questions (no correct
/// flags), the student's current draft answers, and the server-authoritative remaining time. The
/// client renders a countdown from <see cref="RemainingSeconds"/> but the server enforces the
/// deadline.
/// </summary>
public sealed record AttemptTakingDto
{
    public Guid PublicId { get; init; }
    public Guid AssignmentPublicId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? DeadlineAt { get; init; }

    /// <summary>Seconds left until the deadline (null = no time limit). Never negative.</summary>
    public long? RemainingSeconds { get; init; }

    public decimal? TotalPoint { get; init; }
    public IReadOnlyList<TakingQuestionDto> Questions { get; init; } = [];
}

/// <summary>One question as shown to the student (options carry no correctness).</summary>
public sealed record TakingQuestionDto
{
    public Guid PublicId { get; init; }
    public int DisplayPosition { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public IReadOnlyList<TakingOptionDto> Options { get; init; } = [];

    // The student's current draft answer (restored on reload).
    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = [];
    public string? TextAnswer { get; init; }
}

public sealed record TakingOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

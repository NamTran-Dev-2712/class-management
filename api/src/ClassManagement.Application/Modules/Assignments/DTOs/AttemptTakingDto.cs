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

    // Proctoring (MVP-10). The client enables lockdown from Proctoring and seeds its violation counter
    // from ViolationCount (so a reload never resets it). IsLocked → show the lock screen.
    public ProctoringConfigDto Proctoring { get; init; } = new();
    public int ViolationCount { get; init; }
    public bool IsLocked { get; init; }
}

/// <summary>Proctoring rules the student's browser must enforce for this attempt (MVP-10).</summary>
public sealed record ProctoringConfigDto
{
    public bool RequireFullscreen { get; init; }
    public bool DetectTabSwitch { get; init; }
    public bool BlockCopyPaste { get; init; }

    /// <summary>Violation threshold; <c>0</c> = unlimited / log-only.</summary>
    public int MaxViolations { get; init; }
    public string ViolationAction { get; init; } = string.Empty;

    /// <summary>True when any signal must be monitored/sent for this attempt.</summary>
    public bool Enabled => RequireFullscreen || DetectTabSwitch || BlockCopyPaste;
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

    // Frozen question-level media attachments (MVP-9). Inline media is already embedded in Content.
    public IReadOnlyList<TakingMediaDto> Media { get; init; } = [];

    // The student's current draft answer (restored on reload).
    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = [];
    public string? TextAnswer { get; init; }
}

public sealed record TakingOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }

    // Frozen per-option image (MVP-9, T9-04). Null when the option has no image.
    public string? MediaUrl { get; init; }
    public string? MediaKind { get; init; }
}

/// <summary>A frozen media attachment shown during an attempt (MVP-9).</summary>
public sealed record TakingMediaDto
{
    public Guid MediaPublicId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

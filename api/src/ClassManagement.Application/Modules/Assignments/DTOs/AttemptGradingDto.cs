namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// The teacher's grading view of one attempt (MVP-6): attempt meta + every snapshot question in display
/// order with the student's answer. Objective questions carry their auto score + the option list (with
/// correct/selected flags); writing questions carry the student's text answer + the current manual
/// score/feedback (null until graded). The teacher always sees real scores regardless of release.
/// </summary>
public sealed record AttemptGradingDto
{
    public Guid PublicId { get; init; }
    public Guid AssignmentPublicId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public Guid StudentPublicId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? SubmittedAt { get; init; }
    public bool AutoSubmitted { get; init; }

    public decimal? TotalPoint { get; init; }
    public decimal? TotalAutoScore { get; init; }
    public decimal? TotalManualScore { get; init; }
    public decimal? TotalScore { get; init; }

    public IReadOnlyList<GradingQuestionDto> Questions { get; init; } = [];
}

/// <summary>One snapshot question with the student's answer, for grading.</summary>
public sealed record GradingQuestionDto
{
    public Guid QuestionPublicId { get; init; }
    public int DisplayPosition { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public bool IsWriting { get; init; }

    // Objective
    public decimal? AutoScore { get; init; }
    public IReadOnlyList<GradingOptionDto> Options { get; init; } = [];

    // Writing
    public string? TextAnswer { get; init; }
    public decimal? ManualScore { get; init; }
    public string? Feedback { get; init; }
}

/// <summary>A snapshot option shown to the teacher with correct/selected flags.</summary>
public sealed record GradingOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public bool IsSelected { get; init; }
}

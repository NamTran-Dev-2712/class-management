namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// Full assignment view for the owning teacher / admin: all config plus, once published, the frozen
/// snapshot question summaries. While Draft, <see cref="Questions"/> is empty and the FE links to the
/// source exam via <see cref="ExamPublicId"/>.
/// </summary>
public sealed record AssignmentDetailDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;

    public DateTime? OpensAt { get; init; }
    public DateTime? ClosesAt { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int MaxAttempts { get; init; }
    public string ScorePolicy { get; init; } = string.Empty;
    public bool AllowLate { get; init; }
    public string GradePublishPolicy { get; init; } = string.Empty;
    public bool ShuffleQuestions { get; init; }
    public bool ShuffleOptions { get; init; }
    public bool ShowAnswersAfterGrade { get; init; }

    public DateTime? PublishedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public int? ExamVersionAtPublish { get; init; }

    public Guid ExamPublicId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid TeacherPublicId { get; init; }
    public string TeacherName { get; init; } = string.Empty;

    public decimal? TotalPoint { get; init; }
    public int? TotalQuestions { get; init; }
    public int AttemptCount { get; init; }
    public int SubmittedCount { get; init; }

    public IReadOnlyList<AssignmentSnapshotQuestionDto> Questions { get; init; } = [];

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>A frozen snapshot question summary shown in the teacher/admin detail (with correct flags).</summary>
public sealed record AssignmentSnapshotQuestionDto
{
    public Guid PublicId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public int DisplayOrder { get; init; }
    public string? Explanation { get; init; }
    public IReadOnlyList<AssignmentSnapshotOptionDto> Options { get; init; } = [];
}

public sealed record AssignmentSnapshotOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public int DisplayOrder { get; init; }
}

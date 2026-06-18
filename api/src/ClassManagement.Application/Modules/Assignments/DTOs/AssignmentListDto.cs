namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>Row for teacher/admin assignment lists. Read from <c>vw_assignments</c>.</summary>
public sealed record AssignmentListDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? OpensAt { get; init; }
    public DateTime? ClosesAt { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int MaxAttempts { get; init; }
    public decimal? TotalPoint { get; init; }
    public int? TotalQuestions { get; init; }

    public Guid ExamPublicId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid TeacherPublicId { get; init; }
    public string TeacherName { get; init; } = string.Empty;

    public int AttemptCount { get; init; }
    public int SubmittedCount { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

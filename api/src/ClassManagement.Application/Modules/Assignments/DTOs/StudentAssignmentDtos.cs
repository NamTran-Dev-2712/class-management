namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>Row for a student's assignment list. Read from <c>vw_student_assignments</c>.</summary>
public sealed record StudentAssignmentListDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? OpensAt { get; init; }
    public DateTime? ClosesAt { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int MaxAttempts { get; init; }
    public bool AllowLate { get; init; }

    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;

    public decimal? TotalPoint { get; init; }
    public int? TotalQuestions { get; init; }

    public int UsedAttempts { get; init; }
    public int AttemptsLeft { get; init; }
    public bool HasInProgress { get; init; }
    public Guid? InProgressAttemptPublicId { get; init; }
    public decimal? BestScore { get; init; }
}

/// <summary>Detailed assignment view for a student (info needed before starting an attempt).</summary>
public sealed record StudentAssignmentDetailDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? OpensAt { get; init; }
    public DateTime? ClosesAt { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int MaxAttempts { get; init; }
    public bool AllowLate { get; init; }
    public string GradePublishPolicy { get; init; } = string.Empty;

    public Guid ClassPublicId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;

    public decimal? TotalPoint { get; init; }
    public int? TotalQuestions { get; init; }

    public int UsedAttempts { get; init; }
    public int AttemptsLeft { get; init; }
    public bool HasInProgress { get; init; }
    public Guid? InProgressAttemptPublicId { get; init; }
    public bool CanStart { get; init; }
}

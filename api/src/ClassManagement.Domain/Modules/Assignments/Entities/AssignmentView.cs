namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Read-only projection of an assignment for teacher/admin list + detail screens. Mapped to
/// <c>vw_assignments</c> (assignment + class name + exam title + owner display name + snapshot totals +
/// attempt counts, excluding soft-deleted rows) so list queries reuse <c>BaseGetQueryHandler</c>
/// without joining the Identity user type. Enum columns are exposed as plain <c>string</c>. Never
/// written through — mutations go via <c>IAssignmentRepository</c>.
/// </summary>
public sealed class AssignmentView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public string ScorePolicy { get; set; } = string.Empty;
    public bool AllowLate { get; set; }
    public string GradePublishPolicy { get; set; } = string.Empty;
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowAnswersAfterGrade { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? GradesReleasedAt { get; set; }
    public int? ExamVersionAtPublish { get; set; }
    public DateTime UpdatedAt { get; set; }

    public long ExamId { get; set; }
    public Guid ExamPublicId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;

    public long ClassId { get; set; }
    public Guid ClassPublicId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public long TeacherId { get; set; }
    public Guid TeacherPublicId { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    // From the snapshot (null while Draft).
    public decimal? TotalPoint { get; set; }
    public int? TotalQuestions { get; set; }

    public int AttemptCount { get; set; }
    public int SubmittedCount { get; set; }
}

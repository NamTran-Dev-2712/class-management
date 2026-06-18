namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// Per-(assignment, approved-student) projection for the student assignment list/detail. Mapped to
/// <c>vw_student_assignments</c> which joins published assignments to the approved memberships of
/// their class, so a student naturally sees only assignments from classes they belong to. Carries the
/// student's own attempt stats (used attempts, whether one is in progress, best score). Scoped to the
/// current student in the handler's <c>GetBaseQuery</c>. Never written through.
/// </summary>
public sealed class StudentAssignmentView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int MaxAttempts { get; set; }
    public bool AllowLate { get; set; }
    public string GradePublishPolicy { get; set; } = string.Empty;

    public Guid ClassPublicId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    public decimal? TotalPoint { get; set; }
    public int? TotalQuestions { get; set; }

    public long StudentId { get; set; }
    public Guid StudentPublicId { get; set; }

    public int UsedAttempts { get; set; }
    public bool HasInProgress { get; set; }
    public Guid? InProgressAttemptPublicId { get; set; }
    public decimal? BestScore { get; set; }
}

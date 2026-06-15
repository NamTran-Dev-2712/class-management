namespace ClassManagement.Domain.Modules.Classroom.Entities;

/// <summary>
/// Read-only projection of a class membership joined to its student, class, and class owner. Mapped
/// to the <c>vw_class_members</c> view so membership/roster/“my classes” list queries can reuse
/// <c>BaseGetQueryHandler</c> without joining the Identity <c>ApplicationUser</c> type. One row per
/// membership row (all statuses). Never written through — mutations go via
/// <c>IClassMembershipRepository</c>.
/// </summary>
public sealed class ClassMemberView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? RejectionReason { get; set; }

    public long ClassId { get; set; }
    public Guid ClassPublicId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? SubjectName { get; set; }
    public string ClassStatus { get; set; } = string.Empty;

    public long OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;

    public long StudentId { get; set; }
    public Guid StudentPublicId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
}

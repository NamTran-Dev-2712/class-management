namespace ClassManagement.Domain.Modules.Classroom.Entities;

/// <summary>
/// Read-only projection of a class for list/detail screens. Mapped to the <c>vw_classes</c> view
/// (classes + owner display name/email + approved/pending member counts, excluding soft-deleted
/// rows) so list queries can reuse <c>BaseGetQueryHandler</c> without joining the Identity
/// <c>ApplicationUser</c> type. Never written through — all mutations go via <c>IClassRepository</c>.
/// </summary>
public sealed class ClassView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? SubjectId { get; set; }
    public Guid? SubjectPublicId { get; set; }
    public string? SubjectName { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public DateTime UpdatedAt { get; set; }

    public long OwnerId { get; set; }
    public Guid OwnerPublicId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;

    public int ApprovedMemberCount { get; set; }
    public int PendingCount { get; set; }
}

using ClassManagement.Domain.Modules.Classroom.Enums;

namespace ClassManagement.Domain.Modules.Classroom.Entities;

public sealed class Class : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? SubjectId { get; set; }
    public long OwnerId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public ClassStatus Status { get; set; } = ClassStatus.Active;
    public string? CoverImageUrl { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<ClassMembership> Memberships { get; set; } = [];
}

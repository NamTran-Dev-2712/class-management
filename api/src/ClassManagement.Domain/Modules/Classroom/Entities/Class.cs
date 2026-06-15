using ClassManagement.Domain.Modules.Classroom.Enums;

namespace ClassManagement.Domain.Modules.Classroom.Entities;

public sealed class Class : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Subject is an optional label. When the teacher picks a catalog subject, SubjectId links it and
    // SubjectName snapshots its name; when the teacher types a free-text subject, SubjectId is null and
    // SubjectName holds the text; no subject = both null. Lists read SubjectName (no JOIN, survives a
    // catalog rename/delete).
    public long? SubjectId { get; set; }
    public string? SubjectName { get; set; }

    public long OwnerId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public ClassStatus Status { get; set; } = ClassStatus.Active;
    public string? CoverImageUrl { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<ClassMembership> Memberships { get; set; } = [];
}

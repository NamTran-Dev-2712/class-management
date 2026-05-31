namespace ClassManagement.Domain.Modules.Catalog.Entities;

public sealed class Subject : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime? DeletedAt { get; set; }
}

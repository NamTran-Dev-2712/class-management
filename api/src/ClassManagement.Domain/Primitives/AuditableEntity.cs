namespace ClassManagement.Domain.Primitives;

public abstract class AuditableEntity : BaseEntity, IFullyAuditable
{
    public DateTime UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}

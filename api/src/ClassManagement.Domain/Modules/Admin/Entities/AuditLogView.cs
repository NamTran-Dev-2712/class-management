namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// Read-model over <c>vw_audit_logs</c> (MVP-7): audit rows joined to the actor user for display
/// (name/email/public id). Internal ids are never projected to the API. <see cref="Metadata"/> is the
/// raw JSON text of the entry's context.
/// </summary>
public sealed class AuditLogView : BaseEntity
{
    public string Action { get; set; } = string.Empty;

    public long? ActorId { get; set; }
    public Guid? ActorPublicId { get; set; }
    public string? ActorName { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorRole { get; set; }

    public string? TargetType { get; set; }
    public Guid? TargetPublicId { get; set; }

    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// Append-only record of a sensitive action (MVP-7). Never updated, never deleted. Target is
/// polymorphic (no FK on <see cref="ActorId"/>/<see cref="TargetId"/>). <see cref="Metadata"/> carries
/// context such as old/new values for an edit. Use <c>AuditActions</c>/<c>AuditTargetTypes</c> constants
/// for the string fields. Built and staged by <c>IAuditLogger</c>; committed in the same unit of work
/// as the action it records.
/// </summary>
public sealed class AuditLog : BaseEntity
{
    public string Action { get; set; } = string.Empty;

    /// <summary>Actor user id; <c>null</c> for system/background-job actions.</summary>
    public long? ActorId { get; set; }

    /// <summary>Actor role at action time (Student/Teacher/Admin/System).</summary>
    public string? ActorRole { get; set; }

    public string? TargetType { get; set; }
    public long? TargetId { get; set; }
    public Guid? TargetPublicId { get; set; }

    /// <summary>JSONB context (old_value/new_value/reason…). Never store secrets here.</summary>
    public Dictionary<string, object?>? Metadata { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

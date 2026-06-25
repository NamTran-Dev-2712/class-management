namespace ClassManagement.Application.Interfaces.Audit;

/// <summary>
/// One audit entry to record. Actor/role default to the current authenticated user (and IP/User-Agent
/// are filled from the request); override <see cref="ActorId"/>/<see cref="ActorRole"/> for flows where
/// the HTTP principal is not yet set (e.g. login) or for system/background actions.
/// </summary>
public sealed record AuditEntry
{
    public required string Action { get; init; }
    public long? ActorId { get; init; }
    public string? ActorRole { get; init; }
    public string? TargetType { get; init; }
    public long? TargetId { get; init; }
    public Guid? TargetPublicId { get; init; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

/// <summary>
/// Writes append-only audit log entries (MVP-7). <see cref="LogAsync"/> stages the row in the current
/// unit of work so it commits transactionally with the action that produced it; <see cref="LogAndSaveAsync"/>
/// commits it on its own (for flows without a following SaveChanges, e.g. the auth path and the audit
/// pipeline behavior).
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task LogAndSaveAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

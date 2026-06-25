namespace ClassManagement.Application.Interfaces.Audit;

/// <summary>
/// Marks a command whose successful execution should be coarsely audited by <c>AuditLoggingBehavior</c>
/// (MVP-7). Use this for straightforward CRUD-style actions; handlers that need rich old/new metadata
/// call <see cref="IAuditLogger"/> directly instead. The behavior records the action + actor only;
/// <see cref="GetAuditMetadata"/> may add context derived from the request.
/// </summary>
public interface IAuditableRequest
{
    /// <summary>One of <c>AuditActions</c>; recorded as <c>audit_logs.action</c>.</summary>
    string AuditAction { get; }

    /// <summary>One of <c>AuditTargetTypes</c>, or null if not applicable.</summary>
    string? AuditTargetType => null;

    /// <summary>
    /// The target's public id when the request carries it (e.g. update/delete). For create commands
    /// the behavior falls back to a <see cref="System.Guid"/> response value.
    /// </summary>
    Guid? AuditTargetPublicId => null;

    /// <summary>Optional extra context merged into <c>audit_logs.metadata</c>.</summary>
    IReadOnlyDictionary<string, object?>? GetAuditMetadata() => null;
}

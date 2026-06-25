using ClassManagement.Application.Interfaces.Audit;
using ClassManagement.Domain.Modules.Admin.Constants;

namespace ClassManagement.Infrastructure.Services.Audit;

// Builds + persists audit_logs rows (MVP-7). Actor/role/IP/User-Agent default to the current request;
// callers may override actor for the auth path (principal not set yet) or system/background jobs.
public sealed class AuditLogger : IAuditLogger
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AuditLogger(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default) =>
        await _unitOfWork.AuditLogs.AddAsync(Build(entry), cancellationToken);

    public async Task LogAndSaveAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default
    )
    {
        await _unitOfWork.AuditLogs.AddAsync(Build(entry), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private AuditLog Build(AuditEntry entry)
    {
        var actorId = entry.ActorId ?? _currentUser.UserId;
        var actorRole =
            entry.ActorRole
            ?? _currentUser.Role
            ?? (actorId is null ? AuditActorRoles.System : null);

        return new AuditLog
        {
            Action = entry.Action,
            ActorId = actorId,
            ActorRole = actorRole,
            TargetType = entry.TargetType,
            TargetId = entry.TargetId,
            TargetPublicId = entry.TargetPublicId,
            Metadata = entry.Metadata is null
                ? null
                : new Dictionary<string, object?>(entry.Metadata),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
        };
    }
}

using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;

// Teacher/Admin unlocks an attempt that ViolationAction=LockAttempt locked (MVP-10). The student may then
// resume (still InProgress, server timer keeps running). Audited. OwnerScoped = true for the teacher
// surface. AttemptId comes from the route.
public record UnlockAttemptCommand(Guid AttemptId, bool OwnerScoped) : IRequest;

public class UnlockAttemptCommandHandler : IRequestHandler<UnlockAttemptCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public UnlockAttemptCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task Handle(UnlockAttemptCommand request, CancellationToken ct)
    {
        var attempt = await AttemptModeration.ResolveAsync(
            _unitOfWork,
            _currentUser.UserId,
            request.AttemptId,
            request.OwnerScoped,
            ct
        );

        if (attempt.Status != AttemptStatus.InProgress)
            throw new BadException("Attempt.NotInProgress");

        if (!attempt.IsLocked)
            return; // idempotent no-op

        attempt.IsLocked = false;
        _unitOfWork.Attempts.Update(attempt);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.AttemptUnlocked,
                TargetType = AuditTargetTypes.Attempt,
                TargetId = attempt.Id,
                TargetPublicId = attempt.PublicId,
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }
}

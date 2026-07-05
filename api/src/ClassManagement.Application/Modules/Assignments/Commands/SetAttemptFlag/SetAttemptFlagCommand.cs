using ClassManagement.Domain.Modules.Admin.Constants;

// Teacher/Admin flags or clears the flag on an attempt for integrity review (MVP-10, T10-06). Idempotent;
// audited. OwnerScoped = true for the teacher surface. AttemptId comes from the route.
public record SetAttemptFlagCommand(Guid AttemptId, bool Flagged, bool OwnerScoped) : IRequest;

public class SetAttemptFlagCommandHandler : IRequestHandler<SetAttemptFlagCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public SetAttemptFlagCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task Handle(SetAttemptFlagCommand request, CancellationToken ct)
    {
        var attempt = await AttemptModeration.ResolveAsync(
            _unitOfWork,
            _currentUser.UserId,
            request.AttemptId,
            request.OwnerScoped,
            ct
        );

        if (attempt.IsFlagged == request.Flagged)
            return; // idempotent no-op

        attempt.IsFlagged = request.Flagged;
        _unitOfWork.Attempts.Update(attempt);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = request.Flagged
                    ? AuditActions.AttemptFlagged
                    : AuditActions.AttemptUnflagged,
                TargetType = AuditTargetTypes.Attempt,
                TargetId = attempt.Id,
                TargetPublicId = attempt.PublicId,
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }
}

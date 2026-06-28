using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;

// Teacher cancels auto-renew (MVP-8 T8-05). The subscription stays Active until expires_at (cancelled_at
// is set so the lifecycle sweep ends it as Cancelled instead of attempting renewal).
public record CancelSubscriptionCommand : IRequest;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _audit;

    public CancelSubscriptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var subscription =
            await _unitOfWork.Subscriptions.GetActiveByTeacherAsync(teacherId, ct)
            ?? throw new BadException("Subscription.NotCancellable");

        if (subscription.CancelledAt is not null)
            return; // already cancelled — idempotent

        subscription.CancelledAt = DateTime.UtcNow;
        _unitOfWork.Subscriptions.Update(subscription);

        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.SubscriptionCancelled,
                TargetType = AuditTargetTypes.Subscription,
                TargetId = subscription.Id,
                TargetPublicId = subscription.PublicId,
            },
            ct
        );
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

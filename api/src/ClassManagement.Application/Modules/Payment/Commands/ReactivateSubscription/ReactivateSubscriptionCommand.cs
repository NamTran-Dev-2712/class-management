using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;

// Teacher un-cancels a still-Active subscription before it expires (MVP-8 edge case). Clears cancelled_at
// so auto-renew resumes. To extend the period, the teacher checks out again instead.
public record ReactivateSubscriptionCommand : IRequest;

public class ReactivateSubscriptionCommandHandler : IRequestHandler<ReactivateSubscriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _audit;

    public ReactivateSubscriptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task Handle(ReactivateSubscriptionCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var subscription =
            await _unitOfWork.Subscriptions.GetActiveByTeacherAsync(teacherId, ct)
            ?? throw new BadException("Subscription.NotFound");

        if (subscription.CancelledAt is null)
            throw new BadException("Subscription.AlreadyActive");

        subscription.CancelledAt = null;
        _unitOfWork.Subscriptions.Update(subscription);

        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.SubscriptionReactivated,
                TargetType = AuditTargetTypes.Subscription,
                TargetId = subscription.Id,
                TargetPublicId = subscription.PublicId,
            },
            ct
        );
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

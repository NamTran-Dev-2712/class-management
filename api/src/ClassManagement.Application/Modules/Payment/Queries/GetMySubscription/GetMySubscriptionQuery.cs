using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;

// The current teacher's subscription status (MVP-8). Returns a Free representation when there is no
// Active/PastDue subscription.
public record GetMySubscriptionQuery : IRequest<SubscriptionDto>;

public class GetMySubscriptionQueryHandler
    : IRequestHandler<GetMySubscriptionQuery, SubscriptionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMySubscriptionQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto> Handle(GetMySubscriptionQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var subscription = await _unitOfWork.Subscriptions.GetEntitlingByTeacherAsync(
            teacherId,
            ct
        );
        if (subscription is null)
            return new SubscriptionDto(
                null,
                "Free",
                false,
                SubscriptionStatus.Active.ToString(),
                null,
                null,
                null,
                null,
                null,
                SubscriptionPaymentType.Auto.ToString()
            );

        var plan = await _unitOfWork.Plans.GetByIdAsync(subscription.PlanId, ct);
        return new SubscriptionDto(
            subscription.PublicId,
            plan?.Name ?? "Pro",
            (plan?.PriceVnd ?? 0) > 0,
            subscription.Status.ToString(),
            plan?.BillingCycle?.ToString(),
            subscription.StartedAt,
            subscription.ExpiresAt,
            subscription.CancelledAt,
            subscription.GracePeriodEndsAt,
            subscription.PaymentType.ToString()
        );
    }
}

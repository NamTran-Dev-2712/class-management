using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;
using ClassManagement.Domain.Modules.Admin.Constants;

public class CreatePaymentIntentCommandHandler
    : IRequestHandler<CreatePaymentIntentCommand, CheckoutResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentProviderResolver _providers;
    private readonly ISubscriptionPolicy _policy;
    private readonly IAuditLogger _audit;

    public CreatePaymentIntentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPaymentProviderResolver providers,
        ISubscriptionPolicy policy,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _providers = providers;
        _policy = policy;
        _audit = audit;
    }

    public async Task<CheckoutResultDto> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken ct
    )
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var plan =
            await _unitOfWork.Plans.GetByPublicIdAsync(request.PlanPublicId, ct)
            ?? throw new NotFoundException("Plan.NotFound");
        if (!plan.IsActive)
            throw new BadException("Plan.Inactive");
        // The Free plan (price 0 / no cycle) can't be purchased.
        if (plan.PriceVnd <= 0 || plan.BillingCycle is null)
            throw new BadException("Plan.NotPurchasable");

        // Opaque, alphanumeric order reference echoed back by the gateway; the idempotency key dedups
        // webhook retries (provider prefix + ref).
        var orderRef = Guid.NewGuid().ToString("N");
        var idempotencyKey = $"{request.Provider.ToString().ToLowerInvariant()}_{orderRef}";
        var timeoutMinutes = await _policy.GetOrderTimeoutMinutesAsync(ct);

        var payment = new Payment
        {
            TeacherId = teacherId,
            PlanId = plan.Id,
            Provider = request.Provider,
            BillingCycle = plan.BillingCycle.Value,
            AmountVnd = plan.PriceVnd, // locked-in price (BR-8-07)
            Status = PaymentStatus.Pending,
            IdempotencyKey = idempotencyKey,
            ProviderOrderId = orderRef,
            ExpiresAt = DateTime.UtcNow.AddMinutes(timeoutMinutes),
        };

        await _unitOfWork.Payments.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct); // materialize PublicId for the return URL

        var orderInfo = $"{plan.Name} {plan.BillingCycle} subscription";
        var providerAdapter = _providers.Resolve(request.Provider);
        var result = await providerAdapter.CreatePaymentAsync(
            new CreatePaymentRequest(
                orderRef,
                plan.PriceVnd,
                orderInfo,
                payment.PublicId,
                _currentUser.IpAddress
            ),
            ct
        );

        payment.ProviderOrderId = result.ProviderOrderId;
        _unitOfWork.Payments.Update(payment);
        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.PaymentCreated,
                TargetType = AuditTargetTypes.Payment,
                TargetId = payment.Id,
                TargetPublicId = payment.PublicId,
                Metadata = new Dictionary<string, object?>
                {
                    ["provider"] = request.Provider.ToString(),
                    ["amountVnd"] = plan.PriceVnd,
                    ["planName"] = plan.Name,
                },
            },
            ct
        );
        await _unitOfWork.SaveChangesAsync(ct);

        return new CheckoutResultDto(
            payment.PublicId,
            request.Provider.ToString(),
            plan.PriceVnd,
            result.RedirectUrl,
            result.QrCodeUrl
        );
    }
}

using System.Globalization;
using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Interfaces.Notifications;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

// Applies a verified payment webhook exactly once, transactionally (BR-8-03, risk "race condition"):
// verify signature → load payment by idempotency key → if already terminal, no-op → else mark
// Completed/Failed, activate/extend the subscription, issue an invoice, audit, and notify — all in one
// DB transaction. The realtime push fires post-commit via RealtimeDispatchBehavior.
public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderResolver _providers;
    private readonly INotificationService _notifications;
    private readonly IAuditLogger _audit;

    public ProcessPaymentWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentProviderResolver providers,
        INotificationService notifications,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
        _notifications = notifications;
        _audit = audit;
    }

    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken ct)
    {
        var provider = _providers.Resolve(request.Provider);
        var parsed = provider.VerifyAndParseWebhook(
            new PaymentWebhookContext(request.RawBody, request.Query)
        );
        if (parsed is null)
            throw new BadException("Payment.InvalidWebhook");

        var idempotencyKey = $"{request.Provider.ToString().ToLowerInvariant()}_{parsed.OrderRef}";

        await _unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var payment = await _unitOfWork.Payments.GetByIdempotencyKeyAsync(
                    idempotencyKey,
                    token
                );
                if (payment is null)
                    throw new NotFoundException("Payment.NotFound");

                // Idempotency: a retried webhook for an already-applied order is a no-op.
                if (payment.Status != PaymentStatus.Pending)
                    return;

                payment.ProviderTransactionId = parsed.ProviderTransactionId;
                payment.ProviderMetadata = parsed.Metadata;

                if (parsed.Outcome == PaymentWebhookOutcome.Failed)
                    await ApplyFailureAsync(payment, token);
                else
                    await ApplySuccessAsync(payment, token);
            },
            ct
        );
    }

    private async Task ApplyFailureAsync(Payment payment, CancellationToken ct)
    {
        payment.Status = PaymentStatus.Failed;
        _unitOfWork.Payments.Update(payment);

        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.PaymentFailed,
                ActorRole = AuditActorRoles.System,
                TargetType = AuditTargetTypes.Payment,
                TargetId = payment.Id,
                TargetPublicId = payment.PublicId,
            },
            ct
        );
        await _notifications.NotifyAsync(
            payment.TeacherId,
            new NotificationContent
            {
                EventType = NotificationEventType.PaymentFailed,
                Title = "Payment failed",
                Body = "Your payment could not be completed. Please try again.",
                Link = "/teacher/subscription",
                ReferenceId = payment.PublicId.ToString(),
            },
            ct
        );
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplySuccessAsync(Payment payment, CancellationToken ct)
    {
        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;

        var plan =
            await _unitOfWork.Plans.GetByIdAsync(payment.PlanId, ct)
            ?? throw new NotFoundException("Plan.NotFound");

        var subscription = await ActivateSubscriptionAsync(payment, ct);
        payment.SubscriptionId = subscription.Id;
        _unitOfWork.Payments.Update(payment);

        var invoiceNumber = await IssueInvoiceAsync(payment, plan, subscription, ct);

        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.PaymentCompleted,
                ActorRole = AuditActorRoles.System,
                TargetType = AuditTargetTypes.Payment,
                TargetId = payment.Id,
                TargetPublicId = payment.PublicId,
                Metadata = new Dictionary<string, object?> { ["amountVnd"] = payment.AmountVnd },
            },
            ct
        );
        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.SubscriptionCreated,
                ActorRole = AuditActorRoles.System,
                TargetType = AuditTargetTypes.Subscription,
                TargetId = subscription.Id,
                TargetPublicId = subscription.PublicId,
            },
            ct
        );
        await _notifications.NotifyAsync(
            payment.TeacherId,
            new NotificationContent
            {
                EventType = NotificationEventType.PaymentSucceeded,
                Title = "Payment successful",
                Body = $"Your {plan.Name} subscription is now active.",
                Link = "/teacher/subscription",
                ReferenceId = payment.PublicId.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["planName"] = plan.Name,
                    ["amountVnd"] = payment.AmountVnd,
                    ["invoiceNumber"] = invoiceNumber,
                },
            },
            ct
        );
        await _unitOfWork.SaveChangesAsync(ct);
    }

    // Create a new Active subscription, or extend the teacher's existing one (renewal/upgrade). Keeping
    // one row honours the single-Active partial unique index (BR-8-01).
    private async Task<Subscription> ActivateSubscriptionAsync(
        Payment payment,
        CancellationToken ct
    )
    {
        var now = DateTime.UtcNow;
        // Reuse an Active or PastDue row (renewal / paying off a grace period) so we keep one row per
        // teacher and honour the single-Active index.
        var existing = await _unitOfWork.Subscriptions.GetEntitlingByTeacherAsync(
            payment.TeacherId,
            ct
        );

        if (existing is not null)
        {
            var basis = existing.ExpiresAt is { } expiry && expiry > now ? expiry : now;
            existing.ExpiresAt = AddPeriod(basis, payment.BillingCycle);
            existing.PlanId = payment.PlanId;
            existing.Status = SubscriptionStatus.Active;
            existing.CancelledAt = null;
            existing.GracePeriodEndsAt = null;
            existing.PaymentType = SubscriptionPaymentType.Auto;
            _unitOfWork.Subscriptions.Update(existing);
            await _unitOfWork.SaveChangesAsync(ct);
            return existing;
        }

        var subscription = new Subscription
        {
            TeacherId = payment.TeacherId,
            PlanId = payment.PlanId,
            Status = SubscriptionStatus.Active,
            StartedAt = now,
            ExpiresAt = AddPeriod(now, payment.BillingCycle),
            PaymentType = SubscriptionPaymentType.Auto,
        };
        await _unitOfWork.Subscriptions.AddAsync(subscription, ct);
        await _unitOfWork.SaveChangesAsync(ct); // materialize Id for the payment link + invoice
        return subscription;
    }

    // Auto-issue the immutable invoice (BR-8-06). Number = INV-YYYYMM-###### from a DB sequence.
    private async Task<string> IssueInvoiceAsync(
        Payment payment,
        Plan plan,
        Subscription subscription,
        CancellationToken ct
    )
    {
        if (await _unitOfWork.Invoices.ExistsForPaymentAsync(payment.Id, ct))
        {
            var existing = await _unitOfWork.Invoices.GetByPaymentIdAsync(payment.Id, ct);
            return existing!.InvoiceNumber;
        }

        var sequence = await _unitOfWork.Invoices.NextNumberSequenceAsync(ct);
        var number = string.Format(
            CultureInfo.InvariantCulture,
            "INV-{0:yyyyMM}-{1:D6}",
            DateTime.UtcNow,
            sequence
        );

        var invoice = new Invoice
        {
            PaymentId = payment.Id,
            SubscriptionId = subscription.Id,
            TeacherId = payment.TeacherId,
            InvoiceNumber = number,
            AmountVnd = payment.AmountVnd,
            PlanName = plan.Name,
            BillingCycle = payment.BillingCycle,
        };
        await _unitOfWork.Invoices.AddAsync(invoice, ct);

        await _audit.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.InvoiceIssued,
                ActorRole = AuditActorRoles.System,
                TargetType = AuditTargetTypes.Payment,
                TargetId = payment.Id,
                TargetPublicId = payment.PublicId,
                Metadata = new Dictionary<string, object?> { ["invoiceNumber"] = number },
            },
            ct
        );
        return number;
    }

    private static DateTime AddPeriod(DateTime from, BillingCycle cycle) =>
        cycle == BillingCycle.Annual ? from.AddYears(1) : from.AddMonths(1);
}

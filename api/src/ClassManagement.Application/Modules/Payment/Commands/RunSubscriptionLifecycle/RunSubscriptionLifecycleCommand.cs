using ClassManagement.Application.Interfaces.Notifications;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

// Subscription lifecycle sweep (MVP-8). Idempotent — safe to re-run: Active past expiry → Cancelled (if
// the teacher cancelled) or PastDue + grace; PastDue past grace → Expired (+notify); stale Pending
// payments → Expired; and an "expiring soon" reminder N days before expiry. Triggered by Hangfire and
// also runnable on demand (tests).
public record RunSubscriptionLifecycleCommand : IRequest;

public class RunSubscriptionLifecycleCommandHandler
    : IRequestHandler<RunSubscriptionLifecycleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionPolicy _policy;
    private readonly INotificationService _notifications;
    private readonly IAuditLogger _audit;

    public RunSubscriptionLifecycleCommandHandler(
        IUnitOfWork unitOfWork,
        ISubscriptionPolicy policy,
        INotificationService notifications,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _policy = policy;
        _notifications = notifications;
        _audit = audit;
    }

    public async Task Handle(RunSubscriptionLifecycleCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var graceDays = await _policy.GetGracePeriodDaysAsync(ct);
        var noticeDays = await _policy.GetExpiringNoticeDaysAsync(ct);

        // 1. Active subscriptions past their expiry: end (Cancelled) if the teacher opted out, else enter
        //    the PastDue grace window.
        var expiredActive = await _unitOfWork.Subscriptions.GetExpiredActiveAsync(now, ct);
        foreach (var subscription in expiredActive)
        {
            if (subscription.CancelledAt is not null)
            {
                subscription.Status = SubscriptionStatus.Cancelled;
            }
            else
            {
                subscription.Status = SubscriptionStatus.PastDue;
                subscription.GracePeriodEndsAt = now.AddDays(graceDays);
            }
            _unitOfWork.Subscriptions.Update(subscription);
        }

        // 2. PastDue subscriptions whose grace ended → Expired (teacher reverts to Free).
        var graceEnded = await _unitOfWork.Subscriptions.GetGraceEndedAsync(now, ct);
        foreach (var subscription in graceEnded)
        {
            subscription.Status = SubscriptionStatus.Expired;
            _unitOfWork.Subscriptions.Update(subscription);
            await _audit.LogAsync(
                new AuditEntry
                {
                    Action = AuditActions.SubscriptionExpired,
                    ActorRole = AuditActorRoles.System,
                    TargetType = AuditTargetTypes.Subscription,
                    TargetId = subscription.Id,
                    TargetPublicId = subscription.PublicId,
                },
                ct
            );
        }

        // 3. Stale Pending payment orders → Expired (order timeout).
        await _unitOfWork.Payments.ExpireStalePendingAsync(now, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Notify the teachers whose subscriptions just expired.
        foreach (var subscription in graceEnded)
        {
            await _notifications.NotifyAndSaveAsync(
                [subscription.TeacherId],
                new NotificationContent
                {
                    EventType = NotificationEventType.SubscriptionExpired,
                    Title = "Subscription expired",
                    Body = "Your Pro subscription has expired. You are back on the Free plan.",
                    Link = "/teacher/subscription",
                    ReferenceId = subscription.PublicId.ToString(),
                },
                ct
            );
        }

        // 5. "Expiring soon" reminders (BR — proactive renewal). Idempotent per (subscription, expiry date).
        var expiringSoon = await _unitOfWork.Subscriptions.GetExpiringSoonAsync(
            now,
            now.AddDays(noticeDays),
            ct
        );
        foreach (var subscription in expiringSoon)
        {
            await _notifications.NotifyAndSaveAsync(
                [subscription.TeacherId],
                new NotificationContent
                {
                    EventType = NotificationEventType.SubscriptionExpiringSoon,
                    Title = "Subscription expiring soon",
                    Body = "Your Pro subscription is expiring soon. Renew to avoid interruption.",
                    Link = "/teacher/subscription",
                    ReferenceId = $"{subscription.PublicId}:{subscription.ExpiresAt:yyyyMMdd}",
                },
                ct
            );
        }
    }
}

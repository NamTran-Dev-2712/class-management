// Application-facing view of subscription/payment tunables (MVP-8). Implemented in Infrastructure: the
// day/minute values are read live from system_settings (cached) with the SubscriptionOptions/PaymentOptions
// value as fallback, so an admin can change them at runtime (mirrors IClassroomPolicy).
public interface ISubscriptionPolicy
{
    /// <summary>Grace days a PastDue subscription keeps Pro before expiring (BR-8-05).</summary>
    Task<int> GetGracePeriodDaysAsync(CancellationToken cancellationToken = default);

    /// <summary>Days before expiry to send the "expiring soon" notification.</summary>
    Task<int> GetExpiringNoticeDaysAsync(CancellationToken cancellationToken = default);

    /// <summary>Minutes a Pending payment order stays valid before it expires.</summary>
    Task<int> GetOrderTimeoutMinutesAsync(CancellationToken cancellationToken = default);
}

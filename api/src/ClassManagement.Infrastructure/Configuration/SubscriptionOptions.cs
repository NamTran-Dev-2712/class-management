namespace ClassManagement.Infrastructure.Configuration;

// Subscription lifecycle tunables (MVP-8). The day-based values are fallbacks; the live source of truth
// is system_settings (subscription_grace_period_days / subscription_expiring_notice_days). The cron is
// read at registration time (Hangfire), so it stays in appsettings only.
public sealed class SubscriptionOptions
{
    public const string SectionName = "Subscription";

    /// <summary>Fallback grace days a PastDue subscription keeps Pro before expiring (BR-8-05).</summary>
    public int GracePeriodDays { get; init; } = 3;

    /// <summary>Fallback days before expiry to send the "expiring soon" notification.</summary>
    public int ExpiringNoticeDays { get; init; } = 3;

    /// <summary>Cron for the subscription lifecycle sweep (default: every 15 minutes).</summary>
    public string LifecycleSweepCron { get; init; } = "*/15 * * * *";
}

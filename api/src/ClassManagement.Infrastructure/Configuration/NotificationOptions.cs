namespace ClassManagement.Infrastructure.Configuration;

public sealed class NotificationOptions
{
    public const string SectionName = "Notification";

    /// <summary>Cron for the assignment due-soon reminder sweep (default: hourly).</summary>
    public string DueSoonSweepCron { get; init; } = "0 * * * *";

    /// <summary>Cron for the notification cleanup job (default: daily at 03:30).</summary>
    public string CleanupCron { get; init; } = "30 3 * * *";
}

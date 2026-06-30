using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds;

// Seeds the default system_settings rows (MVP-7). Idempotent and additive: only inserts keys that are
// missing, so an Admin's later edits are never overwritten and new keys appear on the next startup.
public static class SystemSettingSeeder
{
    private const string Int = SystemSettingValueTypes.Integer;
    private const string Str = SystemSettingValueTypes.String;
    private const string Bool = SystemSettingValueTypes.Boolean;

    // (key, value as raw JSON, value_type, is_public, description)
    private static readonly (
        string Key,
        string Value,
        string ValueType,
        bool IsPublic,
        string Description
    )[] Defaults =
    [
        (
            SystemSettingKeys.MaxClassesPerTeacher,
            "0",
            Int,
            false,
            "Max classes per teacher (0 = unlimited). Admins can lower this to enforce a cap."
        ),
        (
            SystemSettingKeys.MaxQuestionsPerTeacher,
            "0",
            Int,
            false,
            "Max questions per teacher (0 = unlimited)."
        ),
        (
            SystemSettingKeys.MaxExamsPerTeacher,
            "0",
            Int,
            false,
            "Max exams per teacher (0 = unlimited)."
        ),
        (
            SystemSettingKeys.MaxStudentsPerClass,
            "100",
            Int,
            false,
            "Max approved students per class."
        ),
        (
            SystemSettingKeys.MaxAttemptsPerAssignment,
            "10",
            Int,
            false,
            "Hard cap on attempts per assignment."
        ),
        (SystemSettingKeys.InviteCodeLength, "8", Int, false, "Invite code length (6 or 8)."),
        (SystemSettingKeys.RefreshTokenTtlDays, "7", Int, false, "Refresh token lifetime in days."),
        (
            SystemSettingKeys.ResetPasswordTokenTtlMinutes,
            "15",
            Int,
            false,
            "Password-reset OTP lifetime in minutes."
        ),
        (
            SystemSettingKeys.AssignmentDueSoonHours,
            "24",
            Int,
            false,
            "Hours before deadline to send the due-soon reminder."
        ),
        (
            SystemSettingKeys.NotificationCleanupDays,
            "90",
            Int,
            false,
            "Delete archived notifications older than N days."
        ),
        (
            SystemSettingKeys.MaxReportsPerDay,
            "10",
            Int,
            false,
            "Max reports a single user may submit per day."
        ),
        (SystemSettingKeys.AppName, "\"Class Management\"", Str, true, "Application display name."),
        (
            SystemSettingKeys.MaintenanceMode,
            "false",
            Bool,
            true,
            "When true, the app is in maintenance mode."
        ),
        (
            SystemSettingKeys.SubscriptionGracePeriodDays,
            "3",
            Int,
            false,
            "Days a PastDue subscription keeps Pro before expiring (BR-8-05)."
        ),
        (
            SystemSettingKeys.SubscriptionExpiringNoticeDays,
            "3",
            Int,
            false,
            "Days before expiry to send the 'subscription expiring soon' notification."
        ),
        (
            SystemSettingKeys.PaymentOrderTimeoutMinutes,
            "30",
            Int,
            false,
            "Minutes a Pending payment order stays valid before it expires."
        ),
    ];

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        var existingKeys = await context.SystemSettings.Select(s => s.Key).ToListAsync();
        var existing = existingKeys.ToHashSet(StringComparer.Ordinal);

        var toAdd = Defaults
            .Where(d => !existing.Contains(d.Key))
            .Select(d => new SystemSetting
            {
                Key = d.Key,
                Value = d.Value,
                ValueType = d.ValueType,
                IsPublic = d.IsPublic,
                Description = d.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await context.SystemSettings.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} system setting(s)", toAdd.Count);
    }
}

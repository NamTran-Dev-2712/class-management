namespace ClassManagement.Domain.Modules.Admin.Constants;

/// <summary>
/// Canonical <c>system_settings.key</c> values (MVP-7). Seeded with defaults; read (cached) via
/// <c>ISystemSettingsService</c>. Keep in sync with docs/database/08-schema-admin.md seed table.
/// </summary>
public static class SystemSettingKeys
{
    public const string MaxClassesPerTeacher = "max_classes_per_teacher";
    public const string MaxQuestionsPerTeacher = "max_questions_per_teacher";
    public const string MaxExamsPerTeacher = "max_exams_per_teacher";
    public const string MaxStudentsPerClass = "max_students_per_class";
    public const string MaxAttemptsPerAssignment = "max_attempts_per_assignment";
    public const string InviteCodeLength = "invite_code_length";
    public const string RefreshTokenTtlDays = "refresh_token_ttl_days";
    public const string ResetPasswordTokenTtlMinutes = "reset_password_token_ttl_minutes";
    public const string AssignmentDueSoonHours = "assignment_due_soon_hours";
    public const string NotificationCleanupDays = "notification_cleanup_days";
    public const string MaxReportsPerDay = "max_reports_per_day";
    public const string AppName = "app_name";
    public const string MaintenanceMode = "maintenance_mode";
}

/// <summary>Allowed <c>system_settings.value_type</c> values.</summary>
public static class SystemSettingValueTypes
{
    public const string String = "string";
    public const string Integer = "integer";
    public const string Boolean = "boolean";
    public const string Json = "json";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        String,
        Integer,
        Boolean,
        Json,
    };
}

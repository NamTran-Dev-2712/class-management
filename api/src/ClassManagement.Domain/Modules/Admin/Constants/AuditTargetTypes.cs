namespace ClassManagement.Domain.Modules.Admin.Constants;

/// <summary>
/// Canonical <c>audit_logs.target_type</c> / <c>audit_logs.actor_role</c> values (MVP-7). Plain text,
/// polymorphic (no FK). Kept in sync with the CHECK constraints in AuditLogConfiguration.
/// </summary>
public static class AuditTargetTypes
{
    public const string User = "User";
    public const string Class = "Class";
    public const string Question = "Question";
    public const string Exam = "Exam";
    public const string Assignment = "Assignment";
    public const string Attempt = "Attempt";
    public const string ManualGrade = "ManualGrade";
    public const string Report = "Report";
    public const string Subscription = "Subscription";
    public const string Payment = "Payment";
    public const string SystemSetting = "SystemSetting";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        User,
        Class,
        Question,
        Exam,
        Assignment,
        Attempt,
        ManualGrade,
        Report,
        Subscription,
        Payment,
        SystemSetting,
    };
}

/// <summary><c>audit_logs.actor_role</c> values (role captured at action time).</summary>
public static class AuditActorRoles
{
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string Admin = "Admin";
    public const string System = "System";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Student,
        Teacher,
        Admin,
        System,
    };
}

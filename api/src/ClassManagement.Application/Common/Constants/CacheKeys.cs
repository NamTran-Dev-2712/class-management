namespace ClassManagement.Application.Common.Constants;

public static class CacheKeys
{
    public static string UserProfile(long userId) => $"user:profile:{userId}";

    public static string UserRoles(long userId) => $"user:roles:{userId}";

    public static string SubjectList() => "catalog:subjects:active";

    public static string SubjectDetail(Guid publicId) => $"catalog:subjects:{publicId}";

    public static string SystemSettings() => "system:settings:all";
}

/// <summary>
/// Named output-cache policy ids (one per read surface). Applied on GET actions via
/// <c>[OutputCache(PolicyName = …)]</c>; registered in <c>Api/DependencyInjection</c>.
/// </summary>
public static class OutputCachePolicies
{
    public const string SubjectsRead = "subjects-read";
    public const string UsersRead = "users-read";
    public const string AdminClassesRead = "admin-classes-read";
    public const string TeacherClassesRead = "teacher-classes-read";
    public const string StudentClassesRead = "student-classes-read";
    public const string TeacherQuestionsRead = "teacher-questions-read";
    public const string PublicQuestionsRead = "public-questions-read";
    public const string AdminQuestionsRead = "admin-questions-read";
    public const string TeacherExamsRead = "teacher-exams-read";
    public const string PublicExamsRead = "public-exams-read";
    public const string AdminExamsRead = "admin-exams-read";
    public const string TeacherAssignmentsRead = "teacher-assignments-read";
    public const string StudentAssignmentsRead = "student-assignments-read";
    public const string AdminAssignmentsRead = "admin-assignments-read";
    public const string AttemptsRead = "attempts-read";
    public const string AssignmentReportRead = "assignment-report-read";

    // Admin views any assignment's report (shared across admins) — distinct from the owner-scoped
    // teacher report (AssignmentReportRead, PerUser).
    public const string AdminAssignmentReportRead = "admin-assignment-report-read";

    // Admin & moderation (MVP-7)
    public const string AdminAuditLogsRead = "admin-audit-logs-read";
    public const string NotificationsRead = "notifications-read";
    public const string NotificationUnreadCount = "notifications-unread-count";
    public const string AdminReportsRead = "admin-reports-read";
    public const string MyReportsRead = "my-reports-read";
    public const string SystemSettingsRead = "system-settings-read";
    public const string AdminDashboardRead = "admin-dashboard-read";
    public const string PublicConfigRead = "public-config-read";

    // Premium & payment (MVP-8)
    public const string PlansRead = "plans-read";
    public const string TeacherSubscriptionRead = "teacher-subscription-read";
    public const string SubscriptionUsageRead = "subscription-usage-read";
    public const string InvoicesRead = "invoices-read";
    public const string AdminSubscriptionsRead = "admin-subscriptions-read";
    public const string AdminPaymentsRead = "admin-payments-read";
    public const string AdminRevenueRead = "admin-revenue-read";

    // Media & rich content (MVP-9)
    public const string TeacherMediaRead = "teacher-media-read";
    public const string MediaUsageRead = "media-usage-read";
    public const string AdminMediaRead = "admin-media-read";
    public const string AdminStorageOverviewRead = "admin-storage-overview-read";
}

/// <summary>
/// Output-cache tags (one per resource domain). Read policies tag their entries; write controllers
/// evict the matching tag via <c>IOutputCacheStore.EvictByTagAsync</c> on a successful mutation.
/// </summary>
public static class OutputCacheTags
{
    public const string Subjects = "subjects";
    public const string Users = "users";
    public const string Classrooms = "classrooms";
    public const string Questions = "questions";
    public const string Exams = "exams";
    public const string Assignments = "assignments";

    // Admin & moderation (MVP-7)
    public const string AuditLogs = "audit-logs";
    public const string Notifications = "notifications";
    public const string Reports = "reports";
    public const string SystemSettings = "system-settings";
    public const string AdminDashboard = "admin-dashboard";

    // Premium & payment (MVP-8)
    public const string Plans = "plans";
    public const string Subscriptions = "subscriptions";
    public const string Payments = "payments";
    public const string Invoices = "invoices";

    // Media & rich content (MVP-9)
    public const string Media = "media";
}

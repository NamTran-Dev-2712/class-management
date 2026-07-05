namespace ClassManagement.Domain.Modules.Admin.Constants;

/// <summary>
/// Canonical <c>audit_logs.action</c> values (MVP-7). Dotted lowercase, kept in sync with the CHECK
/// constraint in AuditLogConfiguration and docs/database/08-schema-admin.md. Never inline a literal.
/// </summary>
public static class AuditActions
{
    // Auth
    public const string UserLogin = "user.login";
    public const string UserLogout = "user.logout";
    public const string UserLoginFailed = "user.login_failed";
    public const string UserPasswordChanged = "user.password_changed";
    public const string UserPasswordResetRequested = "user.password_reset_requested";
    public const string UserPasswordResetCompleted = "user.password_reset_completed";

    // Admin user management
    public const string UserRoleChanged = "user.role_changed";
    public const string UserLocked = "user.locked";
    public const string UserUnlocked = "user.unlocked";
    public const string UserDeleted = "user.deleted";

    // Question
    public const string QuestionCreated = "question.created";
    public const string QuestionUpdated = "question.updated";
    public const string QuestionCorrectAnswerChanged = "question.correct_answer_changed";
    public const string QuestionDeleted = "question.deleted";
    public const string QuestionVisibilityChanged = "question.visibility_changed";

    // Exam
    public const string ExamCreated = "exam.created";
    public const string ExamUpdated = "exam.updated";
    public const string ExamDeleted = "exam.deleted";
    public const string ExamVisibilityChanged = "exam.visibility_changed";

    // Assignment
    public const string AssignmentPublished = "assignment.published";
    public const string AssignmentClosed = "assignment.closed";
    public const string AssignmentArchived = "assignment.archived";

    // Attempt
    public const string AttemptStarted = "attempt.started";
    public const string AttemptSubmitted = "attempt.submitted";
    public const string AttemptAutoSubmitted = "attempt.auto_submitted";

    // Attempt moderation / proctoring (MVP-10)
    public const string AttemptForceSubmitted = "attempt.force_submitted";
    public const string AttemptFlagged = "attempt.flagged";
    public const string AttemptUnflagged = "attempt.unflagged";
    public const string AttemptUnlocked = "attempt.unlocked";

    // Grade
    public const string GradeManualGraded = "grade.manual_graded";
    public const string GradeManualGradeUpdated = "grade.manual_grade_updated";
    public const string GradePublished = "grade.published";

    // Report
    public const string ReportCreated = "report.created";
    public const string ReportReviewed = "report.reviewed";
    public const string ReportResolved = "report.resolved";
    public const string ReportRejected = "report.rejected";

    // Admin
    public const string AdminSystemSettingsChanged = "admin.system_settings_changed";
    public const string AdminAssignmentForceClosed = "admin.assignment_force_closed";

    // Payment & subscription (MVP-8)
    public const string SubscriptionCreated = "subscription.created";
    public const string SubscriptionCancelled = "subscription.cancelled";
    public const string SubscriptionReactivated = "subscription.reactivated";
    public const string SubscriptionManualSet = "subscription.manual_set";
    public const string SubscriptionExpired = "subscription.expired";
    public const string PaymentCreated = "payment.created";
    public const string PaymentCompleted = "payment.completed";
    public const string PaymentFailed = "payment.failed";
    public const string InvoiceIssued = "invoice.issued";

    /// <summary>All valid action values — drives the CHECK constraint + validation.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        UserLogin,
        UserLogout,
        UserLoginFailed,
        UserPasswordChanged,
        UserPasswordResetRequested,
        UserPasswordResetCompleted,
        UserRoleChanged,
        UserLocked,
        UserUnlocked,
        UserDeleted,
        QuestionCreated,
        QuestionUpdated,
        QuestionCorrectAnswerChanged,
        QuestionDeleted,
        QuestionVisibilityChanged,
        ExamCreated,
        ExamUpdated,
        ExamDeleted,
        ExamVisibilityChanged,
        AssignmentPublished,
        AssignmentClosed,
        AssignmentArchived,
        AttemptStarted,
        AttemptSubmitted,
        AttemptAutoSubmitted,
        AttemptForceSubmitted,
        AttemptFlagged,
        AttemptUnflagged,
        AttemptUnlocked,
        GradeManualGraded,
        GradeManualGradeUpdated,
        GradePublished,
        ReportCreated,
        ReportReviewed,
        ReportResolved,
        ReportRejected,
        AdminSystemSettingsChanged,
        AdminAssignmentForceClosed,
        SubscriptionCreated,
        SubscriptionCancelled,
        SubscriptionReactivated,
        SubscriptionManualSet,
        SubscriptionExpired,
        PaymentCreated,
        PaymentCompleted,
        PaymentFailed,
        InvoiceIssued,
    };
}

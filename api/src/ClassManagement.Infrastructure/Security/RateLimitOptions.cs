namespace ClassManagement.Infrastructure.Security;

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public RateLimitPolicyOptions Login { get; init; } =
        new() { PermitLimit = 5, WindowSeconds = 60 };
    public RateLimitPolicyOptions Register { get; init; } =
        new() { PermitLimit = 3, WindowSeconds = 60 };
    public RateLimitPolicyOptions Refresh { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };
    public RateLimitPolicyOptions ChangePassword { get; init; } =
        new() { PermitLimit = 3, WindowSeconds = 60 };
    public RateLimitPolicyOptions UpdateProfile { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };
    public RateLimitPolicyOptions ForgotPassword { get; init; } =
        new() { PermitLimit = 3, WindowSeconds = 60 };
    public RateLimitPolicyOptions ResetPassword { get; init; } =
        new() { PermitLimit = 5, WindowSeconds = 60 };
    public RateLimitPolicyOptions Logout { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };
    public RateLimitPolicyOptions SubjectWrite { get; init; } =
        new() { PermitLimit = 20, WindowSeconds = 60 };
    public RateLimitPolicyOptions UserWrite { get; init; } =
        new() { PermitLimit = 20, WindowSeconds = 60 };

    // Classroom create/update/archive/member actions.
    public RateLimitPolicyOptions ClassWrite { get; init; } =
        new() { PermitLimit = 30, WindowSeconds = 60 };

    // Joining a class by invite code — tighter to mitigate invite-code brute-forcing (MVP-2 risk).
    public RateLimitPolicyOptions ClassJoin { get; init; } =
        new() { PermitLimit = 5, WindowSeconds = 60 };

    // Question create/update/delete/visibility/duplicate actions (MVP-3).
    public RateLimitPolicyOptions QuestionWrite { get; init; } =
        new() { PermitLimit = 30, WindowSeconds = 60 };

    // Exam create/update/questions/visibility/duplicate/delete actions (MVP-4).
    public RateLimitPolicyOptions ExamWrite { get; init; } =
        new() { PermitLimit = 30, WindowSeconds = 60 };

    // Assignment create/update/publish/close/archive/delete actions (MVP-5, per-IP teacher writes).
    public RateLimitPolicyOptions AssignmentWrite { get; init; } =
        new() { PermitLimit = 30, WindowSeconds = 60 };

    // Starting an attempt (MVP-5) — tight; partitioned per-user (snapshot read + insert is costly).
    public RateLimitPolicyOptions AttemptStart { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };

    // Auto-save of attempt answers (MVP-5) — partitioned per-user so shared-NAT classrooms don't
    // collide; generous enough for a 30s client debounce + save-on-change, blocks per-student spam.
    public RateLimitPolicyOptions AttemptSave { get; init; } =
        new() { PermitLimit = 20, WindowSeconds = 60 };

    // Submitting an attempt (MVP-5) — partitioned per-user.
    public RateLimitPolicyOptions AttemptSubmit { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };

    // Manual grading / publish-grades (MVP-6, per-IP teacher writes) — bursty (grade many attempts).
    public RateLimitPolicyOptions GradeWrite { get; init; } =
        new() { PermitLimit = 60, WindowSeconds = 60 };

    // CSV grade export (MVP-6) — tight: each call materializes the full roster + serializes a file.
    public RateLimitPolicyOptions Export { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };

    // Submitting a report (MVP-7) — per-user; a hard daily cap (anti-spam) is enforced in the handler.
    public RateLimitPolicyOptions ReportWrite { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };

    // Admin moderation actions (MVP-7, per-IP): review report, force-close assignment, announcements.
    public RateLimitPolicyOptions AdminAction { get; init; } =
        new() { PermitLimit = 30, WindowSeconds = 60 };

    // Marking notifications read/archived (MVP-7) — per-user; generous (UI may batch).
    public RateLimitPolicyOptions NotificationWrite { get; init; } =
        new() { PermitLimit = 60, WindowSeconds = 60 };

    // Changing a system setting (MVP-7, per-IP admin) — rare but sensitive.
    public RateLimitPolicyOptions SystemSettingWrite { get; init; } =
        new() { PermitLimit = 20, WindowSeconds = 60 };

    // Creating a payment order / cancel / reactivate (MVP-8) — per-user; tight (each call hits a
    // provider API and creates a DB order).
    public RateLimitPolicyOptions PaymentCheckout { get; init; } =
        new() { PermitLimit = 10, WindowSeconds = 60 };

    // Provider → server payment webhook (MVP-8) — per-IP; generous (providers may retry), signature
    // verification + idempotency guard the endpoint, this only caps abusive floods.
    public RateLimitPolicyOptions PaymentWebhook { get; init; } =
        new() { PermitLimit = 120, WindowSeconds = 60 };

    // Requesting a presigned media upload URL (MVP-9) — per-user; tight, the main anti-abuse gate for
    // large-file spam (each call reserves quota + creates a Pending asset).
    public RateLimitPolicyOptions MediaPresign { get; init; } =
        new() { PermitLimit = 20, WindowSeconds = 60 };

    // Confirming / deleting a media asset (MVP-9) — per-user; generous (a batch upload confirms many).
    public RateLimitPolicyOptions MediaWrite { get; init; } =
        new() { PermitLimit = 60, WindowSeconds = 60 };

    // Dev-only local bytes upload endpoint (MVP-9, Storage:UseFakeProvider) — per-user.
    public RateLimitPolicyOptions MediaLocalUpload { get; init; } =
        new() { PermitLimit = 60, WindowSeconds = 60 };

    // Generous per-IP cap for GET list endpoints — output cache absorbs repeated identical reads, but
    // this throttles param-varying spam that would otherwise bypass the cache and hit the database.
    public RateLimitPolicyOptions Read { get; init; } =
        new() { PermitLimit = 120, WindowSeconds = 60 };

    public static class Policies
    {
        public const string Login = "login";
        public const string Register = "register";
        public const string Refresh = "refresh";
        public const string ChangePassword = "changePassword";
        public const string UpdateProfile = "updateProfile";
        public const string ForgotPassword = "forgotPassword";
        public const string ResetPassword = "resetPassword";
        public const string Logout = "logout";
        public const string SubjectWrite = "subjectWrite";
        public const string UserWrite = "userWrite";
        public const string ClassWrite = "classWrite";
        public const string ClassJoin = "classJoin";
        public const string QuestionWrite = "questionWrite";
        public const string ExamWrite = "examWrite";
        public const string AssignmentWrite = "assignmentWrite";
        public const string AttemptStart = "attemptStart";
        public const string AttemptSave = "attemptSave";
        public const string AttemptSubmit = "attemptSubmit";
        public const string GradeWrite = "gradeWrite";
        public const string Export = "export";
        public const string ReportWrite = "reportWrite";
        public const string AdminAction = "adminAction";
        public const string NotificationWrite = "notificationWrite";
        public const string SystemSettingWrite = "systemSettingWrite";
        public const string PaymentCheckout = "paymentCheckout";
        public const string PaymentWebhook = "paymentWebhook";
        public const string MediaPresign = "mediaPresign";
        public const string MediaWrite = "mediaWrite";
        public const string MediaLocalUpload = "mediaLocalUpload";
        public const string Read = "read";
    }
}

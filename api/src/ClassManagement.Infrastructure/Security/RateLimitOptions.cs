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
    }
}

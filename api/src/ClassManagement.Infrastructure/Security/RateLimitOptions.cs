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

    public static class Policies
    {
        public const string Login = "login";
        public const string Register = "register";
        public const string Refresh = "refresh";
        public const string ChangePassword = "changePassword";
        public const string UpdateProfile = "updateProfile";
    }
}

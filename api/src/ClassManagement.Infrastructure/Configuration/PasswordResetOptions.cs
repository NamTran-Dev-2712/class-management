namespace ClassManagement.Infrastructure.Configuration;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public int OtpLength { get; init; } = 6;
    public int ExpiryMinutes { get; init; } = 15;
    public int MaxAttempts { get; init; } = 5;
}

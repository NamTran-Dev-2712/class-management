namespace ClassManagement.Infrastructure.Configuration;

public sealed class ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}

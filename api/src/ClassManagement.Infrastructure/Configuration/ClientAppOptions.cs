namespace ClassManagement.Infrastructure.Configuration;

public sealed class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    public string BaseUrl { get; init; } = string.Empty;
    public string ResetPasswordPath { get; init; } = "/reset-password";
}

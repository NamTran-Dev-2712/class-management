namespace ClassManagement.Infrastructure.Configuration;

/// <summary>
/// Supported languages for API responses. Bound from the "Localization" config section.
/// </summary>
public sealed class LocalizationOptions
{
    public const string SectionName = "Localization";

    /// <summary>Culture used when the request matches none of the supported cultures.</summary>
    public string DefaultCulture { get; init; } = "en";

    /// <summary>Two-letter culture codes the API can localize into.</summary>
    public string[] SupportedCultures { get; init; } = ["en", "vi"];
}

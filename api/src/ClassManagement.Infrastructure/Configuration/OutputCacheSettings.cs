namespace ClassManagement.Infrastructure.Configuration;

/// <summary>
/// Output-cache tunables (bound from the <c>OutputCache</c> appsettings section). Named
/// <c>OutputCacheSettings</c> to avoid clashing with the framework's <c>OutputCacheOptions</c>.
/// </summary>
public sealed class OutputCacheSettings
{
    public const string SectionName = "OutputCache";

    /// <summary>Master switch for the output-cache middleware (disabled in some test runs).</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>TTL for shared (non-personalized) read policies.</summary>
    public int DefaultExpirySeconds { get; init; } = 60;

    /// <summary>TTL for per-user (personalized) read policies.</summary>
    public int PerUserExpirySeconds { get; init; } = 30;
}

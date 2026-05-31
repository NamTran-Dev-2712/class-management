namespace ClassManagement.Infrastructure.Persistence.Cache;

public sealed class CacheOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
    public TimeSpan DefaultExpiry { get; init; } = TimeSpan.FromHours(1);
}

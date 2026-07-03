namespace ClassManagement.Application.Interfaces.Settings;

/// <summary>
/// Typed, cached read access to <c>system_settings</c> (MVP-7). Values are stored as JSON; callers pass
/// a fallback used when the key is missing or unparsable (so the app keeps working before seeding or if a
/// setting is removed). Backed by Redis with a short TTL; <see cref="InvalidateAsync"/> is called by the
/// admin write path so changes take effect promptly.
/// </summary>
public interface ISystemSettingsService
{
    Task<int> GetIntAsync(string key, int fallback, CancellationToken cancellationToken = default);
    Task<long> GetLongAsync(
        string key,
        long fallback,
        CancellationToken cancellationToken = default
    );
    Task<bool> GetBoolAsync(
        string key,
        bool fallback,
        CancellationToken cancellationToken = default
    );
    Task<string?> GetStringAsync(
        string key,
        string? fallback,
        CancellationToken cancellationToken = default
    );

    Task InvalidateAsync(CancellationToken cancellationToken = default);
}

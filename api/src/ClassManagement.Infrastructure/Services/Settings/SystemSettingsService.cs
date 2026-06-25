using System.Text.Json;
using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Settings;

namespace ClassManagement.Infrastructure.Services.Settings;

// Cached read access to system_settings (MVP-7). The full key→raw-JSON map is cached in Redis with a
// short TTL; the admin write path calls InvalidateAsync. Values are parsed per-call with a caller
// fallback so a missing/seed-pending/garbled key never breaks a feature.
public sealed class SystemSettingsService : ISystemSettingsService
{
    // Operational cache window — settings change rarely and writes invalidate explicitly.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ISystemSettingRepository _repository;
    private readonly ICacheService _cache;

    public SystemSettingsService(ISystemSettingRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<int> GetIntAsync(
        string key,
        int fallback,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await GetRawAsync(key, cancellationToken);
        return TryParse<int>(raw, out var value) ? value : fallback;
    }

    public async Task<bool> GetBoolAsync(
        string key,
        bool fallback,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await GetRawAsync(key, cancellationToken);
        return TryParse<bool>(raw, out var value) ? value : fallback;
    }

    public async Task<string?> GetStringAsync(
        string key,
        string? fallback,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await GetRawAsync(key, cancellationToken);
        return TryParse<string>(raw, out var value) ? value : fallback;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(CacheKeys.SystemSettings(), cancellationToken);

    private async Task<string?> GetRawAsync(string key, CancellationToken ct)
    {
        var map = await GetMapAsync(ct);
        return map.TryGetValue(key, out var raw) ? raw : null;
    }

    private async Task<Dictionary<string, string>> GetMapAsync(CancellationToken ct)
    {
        var cached = await _cache.GetAsync<Dictionary<string, string>>(
            CacheKeys.SystemSettings(),
            ct
        );
        if (cached is not null)
            return cached;

        var settings = await _repository.GetAllAsync(ct);
        var map = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.Ordinal);
        await _cache.SetAsync(CacheKeys.SystemSettings(), map, CacheTtl, ct);
        return map;
    }

    private static bool TryParse<T>(string? rawJson, out T value)
    {
        value = default!;
        if (string.IsNullOrWhiteSpace(rawJson))
            return false;
        try
        {
            var parsed = JsonSerializer.Deserialize<T>(rawJson);
            if (parsed is null)
                return false;
            value = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

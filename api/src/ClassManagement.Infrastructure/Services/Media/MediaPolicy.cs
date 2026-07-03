using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Media;

// Media tunables: per-file caps + Free-tier storage quota from system_settings (live) with the MediaOptions
// appsettings values as fallback (MVP-7.5). The MIME allowlist comes from appsettings only. Scoped because
// it reads the live cached settings. Default Free storage quota falls back to plans seed (500 MB).
public sealed class MediaPolicy : IMediaPolicy
{
    private const long DefaultFreeStorageBytes = 500L * 1024 * 1024;

    private readonly MediaOptions _options;
    private readonly ISystemSettingsService _settings;

    public MediaPolicy(IOptions<MediaOptions> options, ISystemSettingsService settings)
    {
        _options = options.Value;
        _settings = settings;
    }

    public MediaKind? ResolveKind(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return null;

        var ct = contentType.Trim().ToLowerInvariant();
        if (Contains(_options.AllowedImageTypes, ct))
            return MediaKind.Image;
        if (Contains(_options.AllowedAudioTypes, ct))
            return MediaKind.Audio;
        if (Contains(_options.AllowedVideoTypes, ct))
            return MediaKind.Video;
        return null;
    }

    public Task<long> GetMaxBytesForKindAsync(
        MediaKind kind,
        CancellationToken cancellationToken = default
    ) =>
        kind switch
        {
            MediaKind.Image => _settings.GetLongAsync(
                SystemSettingKeys.MaxImageBytes,
                _options.MaxImageBytes,
                cancellationToken
            ),
            MediaKind.Audio => _settings.GetLongAsync(
                SystemSettingKeys.MaxAudioBytes,
                _options.MaxAudioBytes,
                cancellationToken
            ),
            _ => _settings.GetLongAsync(
                SystemSettingKeys.MaxVideoBytes,
                _options.MaxVideoBytes,
                cancellationToken
            ),
        };

    public Task<long> GetFreeStorageQuotaBytesAsync(
        CancellationToken cancellationToken = default
    ) =>
        _settings.GetLongAsync(
            SystemSettingKeys.MaxStorageBytesPerTeacher,
            DefaultFreeStorageBytes,
            cancellationToken
        );

    private static bool Contains(string[] allowed, string contentType) =>
        Array.Exists(
            allowed,
            a => string.Equals(a, contentType, StringComparison.OrdinalIgnoreCase)
        );
}

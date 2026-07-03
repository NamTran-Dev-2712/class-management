namespace ClassManagement.Infrastructure.Configuration;

// Media-domain tunables (MVP-9). The MIME allowlist + per-kind size caps here are the APP-SETTINGS
// FALLBACKS; the live values are read from system_settings via IMediaPolicy (admins tune caps at
// runtime, MVP-7.5 pattern). The cleanup cron is read at Hangfire registration time so it stays in
// appsettings only. Total-storage quota is per-plan (IResourceLimitService), not here.
public sealed class MediaOptions
{
    public const string SectionName = "Media";

    /// <summary>A Pending asset older than this (never confirmed) is swept by the cleanup job (BR-9-08).</summary>
    public int PendingConfirmTtlMinutes { get; init; } = 60;

    /// <summary>Cron for the media-cleanup recurring job (read at registration time).</summary>
    public string CleanupCron { get; init; } = "*/30 * * * *";

    /// <summary>Max rows processed per cleanup sweep (bounded work).</summary>
    public int CleanupBatchSize { get; init; } = 200;

    // Per-kind size caps (bytes). Fallbacks for system_settings max_image/audio/video_bytes.
    public long MaxImageBytes { get; init; } = 5L * 1024 * 1024; // 5 MB
    public long MaxAudioBytes { get; init; } = 20L * 1024 * 1024; // 20 MB
    public long MaxVideoBytes { get; init; } = 100L * 1024 * 1024; // 100 MB

    // MIME allowlist per kind (BR-9-05 / edge case "unsupported format"). SVG is deliberately excluded —
    // it can carry executable script and would be served as a top-level document.
    public string[] AllowedImageTypes { get; init; } =
    ["image/png", "image/jpeg", "image/gif", "image/webp"];
    public string[] AllowedAudioTypes { get; init; } =
    ["audio/mpeg", "audio/mp4", "audio/ogg", "audio/wav", "audio/webm"];
    public string[] AllowedVideoTypes { get; init; } = ["video/mp4", "video/webm", "video/ogg"];
}

// Application-facing view of media tunables (MVP-9). Per-file size caps + the Free-tier total-storage
// quota are read (cached) from system_settings with the appsettings values as fallback so an admin can
// change them live (MVP-7.5 pattern). The MIME allowlist lives in appsettings (it changes rarely and is
// awkward to store as JSON); ResolveKind maps a declared content-type to its MediaKind or null if blocked.
public interface IMediaPolicy
{
    /// <summary>The kind a content-type maps to per the allowlist, or <c>null</c> when not allowed (BR-9-05).</summary>
    MediaKind? ResolveKind(string contentType);

    /// <summary>Max bytes for a single file of this kind (live; appsettings fallback).</summary>
    Task<long> GetMaxBytesForKindAsync(
        MediaKind kind,
        CancellationToken cancellationToken = default
    );

    /// <summary>Free-tier total-storage quota in bytes (live; appsettings fallback). 0 = unlimited.</summary>
    Task<long> GetFreeStorageQuotaBytesAsync(CancellationToken cancellationToken = default);
}

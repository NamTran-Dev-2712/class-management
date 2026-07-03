using ClassManagement.Domain.Modules.Media.Enums;

namespace ClassManagement.Domain.Modules.Media.Entities;

/// <summary>
/// An uploaded media file — image / audio / video (MVP-9). Created <see cref="MediaStatus.Pending"/> at
/// presign time and flipped to <see cref="MediaStatus.Confirmed"/> only after the object is verified on
/// storage (BR-9-03). <see cref="StorageKey"/> is the internal object key used for provider ops
/// (head/delete) and is NEVER exposed through the API; clients only ever see <see cref="PublicId"/> and
/// the CDN <see cref="Url"/> (BR-9-01). Soft-deletable — the physical object is removed later by the
/// cleanup job, which skips assets still pinned by a published snapshot (BR-9-06 / BR-9-08).
/// </summary>
public sealed class MediaAsset : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public long OwnerId { get; set; }

    public StorageProvider Provider { get; set; }

    // Internal object key (e.g. media/{ownerPublicId}/{mediaPublicId}.png). Never returned by the API.
    public string StorageKey { get; set; } = string.Empty;

    // Public CDN URL (PublicBaseUrl + StorageKey), computed at presign time and frozen into snapshots.
    public string Url { get; set; } = string.Empty;

    public MediaKind Kind { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long ByteSize { get; set; }

    // Optional intrinsic dimensions (client-supplied, best-effort) for layout hints.
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; }

    public MediaStatus Status { get; set; } = MediaStatus.Pending;

    public DateTime? DeletedAt { get; set; }
}

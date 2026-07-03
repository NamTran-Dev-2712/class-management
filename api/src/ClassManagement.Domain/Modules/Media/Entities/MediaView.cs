namespace ClassManagement.Domain.Modules.Media.Entities;

/// <summary>
/// Read-only projection of a media asset for the teacher library + admin storage lists. Mapped to the
/// <c>vw_media</c> view (assets + owner display name, excluding soft-deleted rows) so list queries reuse
/// <c>BaseGetQueryHandler</c> without joining the Identity <c>ApplicationUser</c> type. The internal
/// <c>storage_key</c> is deliberately NOT projected (BR-9-01). Enum columns are plain <c>string</c> (the
/// columns are text). Never written through — mutations go via <c>IMediaRepository</c>.
/// </summary>
public sealed class MediaView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public string Provider { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ByteSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public long OwnerId { get; set; }
    public Guid OwnerPublicId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}

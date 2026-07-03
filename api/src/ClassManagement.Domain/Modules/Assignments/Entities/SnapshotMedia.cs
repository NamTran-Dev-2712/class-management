using ClassManagement.Domain.Modules.Media.Enums;

namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// A frozen media reference captured into an assignment snapshot at publish time (MVP-9). Serves two
/// purposes: (1) the immutable record of which media a published question/option displayed, carrying the
/// <see cref="FrozenUrl"/> as it was at publish; (2) a <b>cleanup guard</b> — a soft-deleted
/// <c>MediaAsset</c> whose <see cref="MediaPublicId"/> still appears in any row here is NOT physically
/// removed, so old/in-progress attempts keep working (BR-9-06 / BR-9-08). Append-only, immutable
/// (RESTRICT), like the other snapshot rows. <see cref="SnapshotOptionId"/> is set for option-level media.
/// </summary>
public sealed class SnapshotMedia : BaseEntity
{
    public long SnapshotQuestionId { get; set; }
    public long? SnapshotOptionId { get; set; }

    public Guid MediaPublicId { get; set; }
    public string FrozenUrl { get; set; } = string.Empty;
    public MediaKind Kind { get; set; }
    public MediaRole Role { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime SnapshotCreatedAt { get; set; }
}

using ClassManagement.Domain.Modules.Admin.Enums;

namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// A user-submitted report of problematic content or behaviour (MVP-7). Mutable while under review; the
/// idempotency index keeps one active (Pending/Reviewing) report per (reporter, target). Target is
/// polymorphic (no FK). Admin sets <see cref="AdminAction"/>/<see cref="AdminNote"/> on resolution.
/// <see cref="UpdatedAt"/> is maintained by the <c>set_updated_at</c> trigger.
/// </summary>
public sealed class Report : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long ReporterId { get; set; }

    public ReportTargetType TargetType { get; set; }
    public long TargetId { get; set; }
    public Guid? TargetPublicId { get; set; }

    public ReportReason Reason { get; set; }
    public string? Description { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public long? AdminId { get; set; }
    public ReportAdminAction? AdminAction { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

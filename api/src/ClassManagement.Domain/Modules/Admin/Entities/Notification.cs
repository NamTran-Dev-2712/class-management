using ClassManagement.Domain.Modules.Admin.Enums;

namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// In-app notification for a single recipient (MVP-7). Append-only aside from the status transition
/// Unread → Read → Archived (and <see cref="ReadAt"/>). Created on system events; bulk-inserted for
/// class-wide fan-out. <see cref="Payload"/> carries event-specific data (e.g. reference ids used by the
/// 24h idempotency index).
/// </summary>
public sealed class Notification : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long UserId { get; set; }

    public NotificationEventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }

    /// <summary>Relative client path to navigate to (e.g. <c>/student/assignments/{id}</c>).</summary>
    public string? Link { get; set; }

    public Dictionary<string, object?>? Payload { get; set; }

    /// <summary>
    /// Optional dedup key (e.g. the assignment public id) for idempotent background sends — backs the
    /// partial unique index. <c>null</c> for events that may legitimately recur (e.g. announcements).
    /// </summary>
    public string? ReferenceId { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime? ReadAt { get; set; }
}

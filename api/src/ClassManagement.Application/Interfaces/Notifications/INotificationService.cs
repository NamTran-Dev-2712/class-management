using ClassManagement.Domain.Modules.Admin.Enums;

namespace ClassManagement.Application.Interfaces.Notifications;

/// <summary>
/// The content of a notification, shared across recipients for a fan-out (MVP-7). <see cref="Title"/>/
/// <see cref="Body"/> are a server-rendered fallback (default culture); the client re-localizes from
/// <see cref="EventType"/> + <see cref="Payload"/>. <see cref="ReferenceId"/> (e.g. an entity public id)
/// drives idempotent background sends via the partial unique index.
/// </summary>
public sealed record NotificationContent
{
    public required NotificationEventType EventType { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }
    public string? Link { get; init; }
    public string? ReferenceId { get; init; }
    public IReadOnlyDictionary<string, object?>? Payload { get; init; }
}

/// <summary>
/// Creates in-app notifications (MVP-7). Single + batch (class-wide fan-out). Staged into the current
/// unit of work so they commit with the action that triggered them; the caller owns
/// <c>SaveChangesAsync</c> (or uses <see cref="NotifyAndSaveAsync"/> for fire-and-forget paths such as
/// background jobs). Batch sends skip recipients who already have a notification for the same
/// (event, reference) — idempotency for retried jobs.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(
        long userId,
        NotificationContent content,
        CancellationToken cancellationToken = default
    );

    Task NotifyManyAsync(
        IReadOnlyCollection<long> userIds,
        NotificationContent content,
        CancellationToken cancellationToken = default
    );

    Task NotifyAndSaveAsync(
        IReadOnlyCollection<long> userIds,
        NotificationContent content,
        CancellationToken cancellationToken = default
    );
}

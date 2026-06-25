using ClassManagement.Application.Interfaces.Notifications;

namespace ClassManagement.Infrastructure.Services.Notifications;

// Creates in-app notification rows (MVP-7). Stages into the shared unit of work; the caller commits
// (or uses NotifyAndSaveAsync). Batch sends skip recipients that already have a notification for the
// same (event, reference) so retried background jobs don't duplicate.
public sealed class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeOutbox _realtimeOutbox;

    public NotificationService(IUnitOfWork unitOfWork, IRealtimeOutbox realtimeOutbox)
    {
        _unitOfWork = unitOfWork;
        _realtimeOutbox = realtimeOutbox;
    }

    public async Task NotifyAsync(
        long userId,
        NotificationContent content,
        CancellationToken cancellationToken = default
    )
    {
        await _unitOfWork.Notifications.AddAsync(Build(userId, content), cancellationToken);
        _realtimeOutbox.QueueNotificationsChanged([userId]);
    }

    public async Task NotifyManyAsync(
        IReadOnlyCollection<long> userIds,
        NotificationContent content,
        CancellationToken cancellationToken = default
    )
    {
        if (userIds.Count == 0)
            return;

        var recipients = userIds.Distinct().ToList();

        // Skip anyone who already has this (event, reference) — idempotent re-sends.
        if (!string.IsNullOrEmpty(content.ReferenceId))
        {
            var already = await _unitOfWork.Notifications.GetRecipientsWithReferenceAsync(
                content.EventType,
                content.ReferenceId,
                recipients,
                cancellationToken
            );
            recipients = recipients.Where(id => !already.Contains(id)).ToList();
        }

        if (recipients.Count == 0)
            return;

        var rows = recipients.Select(id => Build(id, content)).ToList();
        await _unitOfWork.Notifications.AddRangeAsync(rows, cancellationToken);
        _realtimeOutbox.QueueNotificationsChanged(recipients);
    }

    public async Task NotifyAndSaveAsync(
        IReadOnlyCollection<long> userIds,
        NotificationContent content,
        CancellationToken cancellationToken = default
    )
    {
        await NotifyManyAsync(userIds, content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Notification Build(long userId, NotificationContent content) =>
        new()
        {
            UserId = userId,
            EventType = content.EventType,
            Title = content.Title,
            Body = content.Body,
            Link = content.Link,
            ReferenceId = content.ReferenceId,
            Payload = content.Payload is null
                ? null
                : new Dictionary<string, object?>(content.Payload),
        };
}

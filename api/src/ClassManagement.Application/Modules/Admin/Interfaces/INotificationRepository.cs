// Thin data-access for notifications (MVP-7). Single + bulk inserts (class-wide fan-out) and the
// user-scoped status mutations. Stages changes except the set-based helpers, which run in their own
// statement (ExecuteUpdate) and return the affected count; callers commit other staged work via UoW.
public interface INotificationRepository
{
    Task<Notification?> GetByPublicIdForUserAsync(
        Guid publicId,
        long userId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IReadOnlyCollection<Notification> notifications,
        CancellationToken cancellationToken = default
    );

    // Idempotency for background jobs: has an equivalent notification been sent recently?
    Task<bool> ExistsRecentAsync(
        long userId,
        NotificationEventType eventType,
        string referenceId,
        DateTime since,
        CancellationToken cancellationToken = default
    );

    // Of the given users, which already have a notification for this (event, reference) — batch dedup.
    Task<HashSet<long>> GetRecipientsWithReferenceAsync(
        NotificationEventType eventType,
        string referenceId,
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default
    );

    Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default);

    // Set-based: mark all of a user's Unread as Read. Returns affected rows.
    Task<int> MarkAllReadAsync(long userId, CancellationToken cancellationToken = default);

    // Maintenance (set-based): archive Read notifications older than the cutoff; returns affected rows.
    Task<int> ArchiveReadOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    );

    // Maintenance (set-based): permanently delete Archived notifications older than the cutoff.
    Task<int> DeleteArchivedOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    );
}

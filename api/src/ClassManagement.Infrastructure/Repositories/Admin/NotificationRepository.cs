using ClassManagement.Infrastructure.Persistence.DbContext;

// Notification data-access over the shared scoped DbContext. Inserts STAGE only; the set-based
// MarkAllReadAsync runs its own UPDATE statement (ExecuteUpdate) immediately.
public sealed class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Notification?> GetByPublicIdForUserAsync(
        Guid publicId,
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Notification>()
            .FirstOrDefaultAsync(
                n => n.PublicId == publicId && n.UserId == userId,
                cancellationToken
            );

    public async Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default
    ) => await _context.Set<Notification>().AddAsync(notification, cancellationToken);

    public async Task AddRangeAsync(
        IReadOnlyCollection<Notification> notifications,
        CancellationToken cancellationToken = default
    ) => await _context.Set<Notification>().AddRangeAsync(notifications, cancellationToken);

    public Task<bool> ExistsRecentAsync(
        long userId,
        NotificationEventType eventType,
        string referenceId,
        DateTime since,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Notification>()
            .AnyAsync(
                n =>
                    n.UserId == userId
                    && n.EventType == eventType
                    && n.ReferenceId == referenceId
                    && n.CreatedAt >= since,
                cancellationToken
            );

    public async Task<HashSet<long>> GetRecipientsWithReferenceAsync(
        NotificationEventType eventType,
        string referenceId,
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _context
            .Set<Notification>()
            .Where(n =>
                n.EventType == eventType
                && n.ReferenceId == referenceId
                && userIds.Contains(n.UserId)
            )
            .Select(n => n.UserId)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default) =>
        _context
            .Set<Notification>()
            .CountAsync(
                n => n.UserId == userId && n.Status == NotificationStatus.Unread,
                cancellationToken
            );

    public Task<int> MarkAllReadAsync(long userId, CancellationToken cancellationToken = default) =>
        _context
            .Set<Notification>()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(n => n.Status, NotificationStatus.Read)
                        .SetProperty(n => n.ReadAt, DateTime.UtcNow),
                cancellationToken
            );

    public Task<int> ArchiveReadOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Notification>()
            .Where(n => n.Status == NotificationStatus.Read && n.CreatedAt < cutoff)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(n => n.Status, NotificationStatus.Archived),
                cancellationToken
            );

    public Task<int> DeleteArchivedOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Notification>()
            .Where(n => n.Status == NotificationStatus.Archived && n.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
}

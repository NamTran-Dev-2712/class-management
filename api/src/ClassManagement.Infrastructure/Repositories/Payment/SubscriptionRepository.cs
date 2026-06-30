using ClassManagement.Infrastructure.Persistence.DbContext;

// Subscription data-access over the shared scoped DbContext. Stages only — the handler commits.
public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Subscription?> GetActiveByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) =>
        _context.Subscriptions.FirstOrDefaultAsync(
            s => s.TeacherId == teacherId && s.Status == SubscriptionStatus.Active,
            cancellationToken
        );

    public Task<Subscription?> GetEntitlingByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Subscriptions.Where(s =>
                s.TeacherId == teacherId
                && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue)
            )
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Subscription?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Subscriptions.FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);

    public Task<List<Subscription>> GetExpiredActiveAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Subscriptions.Where(s =>
                s.Status == SubscriptionStatus.Active && s.ExpiresAt != null && s.ExpiresAt < asOf
            )
            .ToListAsync(cancellationToken);

    public Task<List<Subscription>> GetGraceEndedAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Subscriptions.Where(s =>
                s.Status == SubscriptionStatus.PastDue
                && s.GracePeriodEndsAt != null
                && s.GracePeriodEndsAt < asOf
            )
            .ToListAsync(cancellationToken);

    public Task<List<Subscription>> GetExpiringSoonAsync(
        DateTime from,
        DateTime until,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Subscriptions.Where(s =>
                s.Status == SubscriptionStatus.Active
                && s.CancelledAt == null
                && s.ExpiresAt != null
                && s.ExpiresAt >= from
                && s.ExpiresAt <= until
            )
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        Subscription subscription,
        CancellationToken cancellationToken = default
    ) => await _context.Subscriptions.AddAsync(subscription, cancellationToken);

    public void Update(Subscription subscription) => _context.Subscriptions.Update(subscription);
}

// Thin data-access for the Subscription aggregate (MVP-8). Stages writes only — the handler commits via
// IUnitOfWork.SaveChangesAsync(). Business rules (one Active per teacher, lifecycle) live in handlers.
public interface ISubscriptionRepository
{
    // The teacher's single Active subscription, if any (BR-8-01). NULL ⇒ on Free.
    Task<Subscription?> GetActiveByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    );

    // The subscription that currently grants Pro: Active, or PastDue within its grace period (BR-8-05).
    // NULL ⇒ the teacher is effectively on Free.
    Task<Subscription?> GetEntitlingByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    );

    Task<Subscription?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    );

    // Lifecycle sweep (tracked) — Active rows past expiry, PastDue rows past grace, and rows expiring soon.
    Task<List<Subscription>> GetExpiredActiveAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    );
    Task<List<Subscription>> GetGraceEndedAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    );
    Task<List<Subscription>> GetExpiringSoonAsync(
        DateTime from,
        DateTime until,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    void Update(Subscription subscription);
}

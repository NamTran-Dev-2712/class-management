// Thin data-access for the Payment aggregate (MVP-8). Stages writes only — the handler commits via
// IUnitOfWork.SaveChangesAsync(). The idempotency lookup backs exactly-once webhook processing.
public interface IPaymentRepository
{
    Task<Payment?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    );

    Task<Payment?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    // Set-based: mark Pending orders past their timeout as Expired. Returns the affected row count.
    Task<int> ExpireStalePendingAsync(DateTime asOf, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    void Update(Payment payment);
}

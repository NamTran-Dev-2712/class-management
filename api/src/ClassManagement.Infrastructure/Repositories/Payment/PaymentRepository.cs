using ClassManagement.Infrastructure.Persistence.DbContext;
using PaymentEntity = ClassManagement.Domain.Modules.Payment.Entities.Payment;

// Payment data-access over the shared scoped DbContext. Stages only — the handler commits.
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PaymentEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    ) =>
        _context.Payments.FirstOrDefaultAsync(
            p => p.IdempotencyKey == idempotencyKey,
            cancellationToken
        );

    public Task<PaymentEntity?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Payments.FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

    public Task<int> ExpireStalePendingAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Payments.Where(p =>
                p.Status == PaymentStatus.Pending && p.ExpiresAt != null && p.ExpiresAt < asOf
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Status, PaymentStatus.Expired),
                cancellationToken
            );

    public async Task AddAsync(
        PaymentEntity payment,
        CancellationToken cancellationToken = default
    ) => await _context.Payments.AddAsync(payment, cancellationToken);

    public void Update(PaymentEntity payment) => _context.Payments.Update(payment);
}

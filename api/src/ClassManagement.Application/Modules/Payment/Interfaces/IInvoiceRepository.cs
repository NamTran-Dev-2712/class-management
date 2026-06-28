// Thin data-access for the Invoice aggregate (MVP-8). Append-only — stages inserts only; the handler
// commits via IUnitOfWork.SaveChangesAsync(). Invoice numbers come from a DB sequence (race-free).
public interface IInvoiceRepository
{
    Task<bool> ExistsForPaymentAsync(long paymentId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByPaymentIdAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    );

    Task<Invoice?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    // A teacher's invoices, newest first (history view).
    Task<List<Invoice>> ListByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    );

    // Next value of the global invoice-number sequence (payment_invoice_number_seq).
    Task<long> NextNumberSequenceAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

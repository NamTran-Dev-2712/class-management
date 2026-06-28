using ClassManagement.Infrastructure.Persistence.DbContext;

// Invoice data-access over the shared scoped DbContext. Append-only — stages inserts; the handler
// commits. Invoice numbers come from the global DB sequence (payment_invoice_number_seq), race-free.
public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsForPaymentAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    ) => _context.Invoices.AnyAsync(i => i.PaymentId == paymentId, cancellationToken);

    public Task<Invoice?> GetByPaymentIdAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    ) => _context.Invoices.FirstOrDefaultAsync(i => i.PaymentId == paymentId, cancellationToken);

    public Task<Invoice?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Invoices.FirstOrDefaultAsync(i => i.PublicId == publicId, cancellationToken);

    public Task<List<Invoice>> ListByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Invoices.AsNoTracking()
            .Where(i => i.TeacherId == teacherId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<long> NextNumberSequenceAsync(CancellationToken cancellationToken = default)
    {
        var values = await _context
            .Database.SqlQueryRaw<long>("SELECT nextval('payment_invoice_number_seq') AS \"Value\"")
            .ToListAsync(cancellationToken);
        return values[0];
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await _context.Invoices.AddAsync(invoice, cancellationToken);
}

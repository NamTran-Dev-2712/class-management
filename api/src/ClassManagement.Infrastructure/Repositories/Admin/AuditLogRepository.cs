using ClassManagement.Infrastructure.Persistence.DbContext;

// Append-only writer for audit_logs over the shared scoped DbContext. Stages only — the caller commits.
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) =>
        await _context.Set<AuditLog>().AddAsync(log, cancellationToken);
}

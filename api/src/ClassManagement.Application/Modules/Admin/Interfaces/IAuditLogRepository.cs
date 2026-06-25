// Thin data-access for the append-only audit_logs table (MVP-7). Stages writes only — the caller commits
// through IUnitOfWork.SaveChangesAsync() in the same unit of work as the action being recorded. Reads go
// through the read-model view (vw_audit_logs) via BaseGetQueryHandler, not this repo.
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
}

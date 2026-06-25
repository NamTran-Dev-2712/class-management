// Thin data-access for the Report aggregate (MVP-7). Stages changes only — handlers own the commit via
// IUnitOfWork.SaveChangesAsync(). Business rules (idempotency, daily cap, admin guards) live in handlers.
public interface IReportRepository
{
    Task<Report?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    // The existing active (Pending/Reviewing) report for this (reporter, target), if any — idempotency.
    Task<Report?> FindActiveAsync(
        long reporterId,
        ReportTargetType targetType,
        long targetId,
        CancellationToken cancellationToken = default
    );

    // How many reports this user filed since a cutoff — daily anti-spam cap.
    Task<int> CountByReporterSinceAsync(
        long reporterId,
        DateTime since,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    void Update(Report report);
}

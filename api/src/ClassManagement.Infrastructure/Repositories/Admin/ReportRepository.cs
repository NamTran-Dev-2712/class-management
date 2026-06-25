using ClassManagement.Infrastructure.Persistence.DbContext;

// Report data-access over the shared scoped DbContext. Mutation methods STAGE only — handlers commit
// through IUnitOfWork.SaveChangesAsync().
public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Report?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Set<Report>().FirstOrDefaultAsync(r => r.PublicId == publicId, cancellationToken);

    public Task<Report?> FindActiveAsync(
        long reporterId,
        ReportTargetType targetType,
        long targetId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Report>()
            .FirstOrDefaultAsync(
                r =>
                    r.ReporterId == reporterId
                    && r.TargetType == targetType
                    && r.TargetId == targetId
                    && (r.Status == ReportStatus.Pending || r.Status == ReportStatus.Reviewing),
                cancellationToken
            );

    public Task<int> CountByReporterSinceAsync(
        long reporterId,
        DateTime since,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Set<Report>()
            .CountAsync(r => r.ReporterId == reporterId && r.CreatedAt >= since, cancellationToken);

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default) =>
        await _context.Set<Report>().AddAsync(report, cancellationToken);

    public void Update(Report report) => _context.Set<Report>().Update(report);
}

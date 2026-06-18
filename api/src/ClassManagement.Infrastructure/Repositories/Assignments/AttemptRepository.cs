using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;

// Attempt data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate the aggregate (including the owned Answers collection).
public sealed class AttemptRepository : IAttemptRepository
{
    private readonly ApplicationDbContext _context;

    public AttemptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Attempt?> GetByPublicIdAsync(
        Guid publicId,
        bool includeAnswers = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Attempts.AsQueryable();
        if (includeAnswers)
            query = query.Include(a => a.Answers);

        return query.FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);
    }

    public Task<Attempt?> GetInProgressAsync(
        long assignmentId,
        long studentId,
        CancellationToken cancellationToken = default
    ) =>
        _context.Attempts.FirstOrDefaultAsync(
            a =>
                a.AssignmentId == assignmentId
                && a.StudentId == studentId
                && a.Status == AttemptStatus.InProgress,
            cancellationToken
        );

    public Task<int> CountByStudentAsync(
        long assignmentId,
        long studentId,
        CancellationToken cancellationToken = default
    ) =>
        _context.Attempts.CountAsync(
            a => a.AssignmentId == assignmentId && a.StudentId == studentId,
            cancellationToken
        );

    public async Task<IReadOnlyList<Attempt>> GetInProgressByAssignmentAsync(
        long assignmentId,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .Attempts.Include(a => a.Answers)
            .Where(a => a.AssignmentId == assignmentId && a.Status == AttemptStatus.InProgress)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Attempt>> GetExpiredInProgressAsync(
        DateTime asOf,
        int batchSize,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .Attempts.Include(a => a.Answers)
            .Where(a =>
                a.Status == AttemptStatus.InProgress && a.DeadlineAt != null && a.DeadlineAt <= asOf
            )
            .OrderBy(a => a.DeadlineAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Attempt entity, CancellationToken cancellationToken = default) =>
        await _context.Attempts.AddAsync(entity, cancellationToken);

    public void Update(Attempt entity) => _context.Attempts.Update(entity);
}

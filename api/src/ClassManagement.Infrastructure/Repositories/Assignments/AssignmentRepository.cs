using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;

// Assignment data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate the aggregate (including the owned Snapshot).
public sealed class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public AssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Assignment?> GetByPublicIdAsync(
        Guid publicId,
        bool includeSnapshot = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Assignments.AsQueryable();
        if (includeSnapshot)
            query = query
                .Include(a => a.Snapshot!)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Options);

        return query.FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);
    }

    public Task<int> CountByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => _context.Assignments.CountAsync(a => a.TeacherId == teacherId, cancellationToken);

    public Task<bool> HasAttemptsAsync(
        long assignmentId,
        CancellationToken cancellationToken = default
    ) => _context.Attempts.AnyAsync(a => a.AssignmentId == assignmentId, cancellationToken);

    public async Task<IReadOnlyList<Assignment>> GetScheduledDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .Assignments.Where(a =>
                a.Status == AssignmentStatus.Scheduled && a.OpensAt != null && a.OpensAt <= asOf
            )
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Assignment>> GetOpenClosingDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .Assignments.Where(a =>
                a.Status == AssignmentStatus.Open && a.ClosesAt != null && a.ClosesAt <= asOf
            )
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Assignment entity, CancellationToken cancellationToken = default) =>
        await _context.Assignments.AddAsync(entity, cancellationToken);

    public void Update(Assignment entity) => _context.Assignments.Update(entity);

    public void Remove(Assignment entity) => _context.Assignments.Remove(entity);
}

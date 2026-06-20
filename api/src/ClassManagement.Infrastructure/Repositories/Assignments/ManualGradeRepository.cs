using ClassManagement.Infrastructure.Persistence.DbContext;

// Manual-grade data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync().
public sealed class ManualGradeRepository : IManualGradeRepository
{
    private readonly ApplicationDbContext _context;

    public ManualGradeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ManualGrade>> GetByAttemptAsync(
        long attemptId,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .ManualGrades.Where(g => g.AttemptId == attemptId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ManualGrade entity, CancellationToken cancellationToken = default) =>
        await _context.ManualGrades.AddAsync(entity, cancellationToken);

    public void Update(ManualGrade entity) => _context.ManualGrades.Update(entity);
}

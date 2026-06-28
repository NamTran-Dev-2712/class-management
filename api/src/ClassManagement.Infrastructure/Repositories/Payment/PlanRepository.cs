using ClassManagement.Infrastructure.Persistence.DbContext;

// Plan data-access over the shared scoped DbContext. Mutation stages only — the handler commits through
// IUnitOfWork.SaveChangesAsync().
public sealed class PlanRepository : IPlanRepository
{
    private readonly ApplicationDbContext _context;

    public PlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Plan?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _context.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Plan?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Plans.FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

    public Task<List<Plan>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        _context
            .Plans.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.PriceVnd)
            .ToListAsync(cancellationToken);

    public void Update(Plan plan) => _context.Plans.Update(plan);
}

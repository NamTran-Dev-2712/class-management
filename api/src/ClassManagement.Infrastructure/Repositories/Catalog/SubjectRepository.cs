using ClassManagement.Infrastructure.Persistence.DbContext;

// Subject data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(), which resolves the same scoped context.
public sealed class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _context;

    public SubjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Subject?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Subjects.FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludePublicId = null,
        CancellationToken cancellationToken = default
    ) =>
        _context.Subjects.AnyAsync(
            s => s.Name == name && (excludePublicId == null || s.PublicId != excludePublicId),
            cancellationToken
        );

    public async Task AddAsync(Subject subject, CancellationToken cancellationToken = default) =>
        await _context.Subjects.AddAsync(subject, cancellationToken);

    public void Update(Subject subject) => _context.Subjects.Update(subject);

    public void Remove(Subject subject) => _context.Subjects.Remove(subject);
}

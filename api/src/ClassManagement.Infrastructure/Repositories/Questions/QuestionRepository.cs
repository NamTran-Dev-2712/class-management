using ClassManagement.Infrastructure.Persistence.DbContext;

// Question data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate the aggregate (including the owned Options/Tags collections).
public sealed class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Question?> GetByPublicIdAsync(
        Guid publicId,
        bool includeChildren = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Questions.AsQueryable();
        if (includeChildren)
            query = query.Include(q => q.Options).Include(q => q.Tags);

        return query.FirstOrDefaultAsync(q => q.PublicId == publicId, cancellationToken);
    }

    public Task<int> CountByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => _context.Questions.CountAsync(q => q.TeacherId == teacherId, cancellationToken);

    public async Task AddAsync(Question entity, CancellationToken cancellationToken = default) =>
        await _context.Questions.AddAsync(entity, cancellationToken);

    public void Update(Question entity) => _context.Questions.Update(entity);

    public void Remove(Question entity) => _context.Questions.Remove(entity);
}

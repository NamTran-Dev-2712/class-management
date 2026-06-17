using ClassManagement.Infrastructure.Persistence.DbContext;

// Exam data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate the aggregate (including the owned Questions collection).
public sealed class ExamRepository : IExamRepository
{
    private readonly ApplicationDbContext _context;

    public ExamRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Exam?> GetByPublicIdAsync(
        Guid publicId,
        bool includeQuestions = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Exams.AsQueryable();
        if (includeQuestions)
            query = query.Include(e => e.Questions);

        return query.FirstOrDefaultAsync(e => e.PublicId == publicId, cancellationToken);
    }

    public Task<int> CountByTeacherAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => _context.Exams.CountAsync(e => e.TeacherId == teacherId, cancellationToken);

    public async Task AddAsync(Exam entity, CancellationToken cancellationToken = default) =>
        await _context.Exams.AddAsync(entity, cancellationToken);

    public void Update(Exam entity) => _context.Exams.Update(entity);

    public void Remove(Exam entity) => _context.Exams.Remove(entity);
}

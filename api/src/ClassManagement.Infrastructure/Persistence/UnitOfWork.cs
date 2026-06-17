using ClassManagement.Infrastructure.Persistence.DbContext;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Repositories are constructor-injected and share this unit's scoped DbContext (both resolve
    // the same pooled instance per request), so their staged changes commit on SaveChangesAsync.
    public UnitOfWork(
        ApplicationDbContext context,
        ISubjectRepository subjects,
        IClassRepository classes,
        IClassMembershipRepository classMemberships,
        IQuestionRepository questions,
        IExamRepository exams
    )
    {
        _context = context;
        Subjects = subjects;
        Classes = classes;
        ClassMemberships = classMemberships;
        Questions = questions;
        Exams = exams;
    }

    public ISubjectRepository Subjects { get; }
    public IClassRepository Classes { get; }
    public IClassMembershipRepository ClassMemberships { get; }
    public IQuestionRepository Questions { get; }
    public IExamRepository Exams { get; }

    public IGenericRepository<T> Repository<T>()
        where T : class
    {
        return new GenericRepository<T>(_context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }
}

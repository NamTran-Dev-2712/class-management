using ClassManagement.Infrastructure.Persistence.DbContext;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Repositories are constructor-injected and share this unit's scoped DbContext (both resolve
    // the same pooled instance per request), so their staged changes commit on SaveChangesAsync.
    public UnitOfWork(ApplicationDbContext context, ISubjectRepository subjects)
    {
        _context = context;
        Subjects = subjects;
    }

    public ISubjectRepository Subjects { get; }

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

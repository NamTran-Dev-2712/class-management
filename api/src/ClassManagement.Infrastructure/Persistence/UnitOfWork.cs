// Infrastructure/Persistence/UnitOfWork.cs
using ClassManagement.Infrastructure.Persistence;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IGenericRepository<T> Repository<T>()
        where T : class
    {
        return new GenericRepository<T>(_context);
    }

    // repositories that require special handling (e.g., involving Identity) can be added as properties
    private IAuthRepository? _authRepository;

    // lazy initialization of the AuthRepository to avoid circular dependencies
    public IAuthRepository Auth => _authRepository ??= new AuthRepository(_userManager, this);

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

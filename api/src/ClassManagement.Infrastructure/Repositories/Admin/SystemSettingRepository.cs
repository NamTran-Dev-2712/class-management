using ClassManagement.Infrastructure.Persistence.DbContext;

// system_settings data-access over the shared scoped DbContext. Mutations STAGE only — the handler/seeder
// commits through IUnitOfWork.SaveChangesAsync(). Caching is handled above this layer.
public sealed class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<SystemSetting?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default
    ) => _context.Set<SystemSetting>().FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

    public async Task<IReadOnlyList<SystemSetting>> GetAllAsync(
        CancellationToken cancellationToken = default
    ) => await _context.Set<SystemSetting>().OrderBy(s => s.Key).ToListAsync(cancellationToken);

    public async Task AddAsync(
        SystemSetting setting,
        CancellationToken cancellationToken = default
    ) => await _context.Set<SystemSetting>().AddAsync(setting, cancellationToken);

    public async Task AddRangeAsync(
        IReadOnlyCollection<SystemSetting> settings,
        CancellationToken cancellationToken = default
    ) => await _context.Set<SystemSetting>().AddRangeAsync(settings, cancellationToken);

    public void Update(SystemSetting setting) => _context.Set<SystemSetting>().Update(setting);
}

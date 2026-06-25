// Thin data-access for system_settings (MVP-7). Stages changes only — handlers/seeder commit via
// IUnitOfWork.SaveChangesAsync(). Read-through caching lives in ISystemSettingsService, not here.
public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IReadOnlyCollection<SystemSetting> settings,
        CancellationToken cancellationToken = default
    );
    void Update(SystemSetting setting);
}

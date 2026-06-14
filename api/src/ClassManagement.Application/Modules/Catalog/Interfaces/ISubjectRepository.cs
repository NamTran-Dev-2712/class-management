// Thin data-access for the Subject aggregate. Stages changes only — the command handler owns the
// commit via IUnitOfWork.SaveChangesAsync() (both share the same scoped DbContext). Business rules
// (name uniqueness, cache invalidation) live in the handlers.
public interface ISubjectRepository
{
    Task<Subject?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    // Name uniqueness among non-deleted rows, optionally excluding a given subject (for updates).
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludePublicId = null,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);
    void Update(Subject subject);
    void Remove(Subject subject);
}

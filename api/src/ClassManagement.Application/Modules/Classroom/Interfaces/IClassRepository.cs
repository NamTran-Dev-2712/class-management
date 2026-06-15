// Class data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate them.
public interface IClassRepository
{
    Task<Class?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Class?> GetByInviteCodeAsync(
        string inviteCode,
        CancellationToken cancellationToken = default
    );

    Task<bool> InviteCodeExistsAsync(
        string inviteCode,
        CancellationToken cancellationToken = default
    );

    Task<bool> NameExistsForOwnerAsync(
        string name,
        long ownerId,
        Guid? excludePublicId = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CountByOwnerAsync(long ownerId, CancellationToken cancellationToken = default);

    Task AddAsync(Class entity, CancellationToken cancellationToken = default);

    void Update(Class entity);
}

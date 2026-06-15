using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;

// Class data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync().
public sealed class ClassRepository : IClassRepository
{
    private readonly ApplicationDbContext _context;

    public ClassRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Class?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.Classes.FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);

    public Task<Class?> GetByInviteCodeAsync(
        string inviteCode,
        CancellationToken cancellationToken = default
    ) => _context.Classes.FirstOrDefaultAsync(c => c.InviteCode == inviteCode, cancellationToken);

    // Invite codes are globally unique including soft-deleted classes (uq_classes_invite_code has no
    // partial filter), so check across ALL rows to avoid colliding with a deleted class's code.
    public Task<bool> InviteCodeExistsAsync(
        string inviteCode,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .Classes.IgnoreQueryFilters()
            .AnyAsync(c => c.InviteCode == inviteCode, cancellationToken);

    public Task<bool> NameExistsForOwnerAsync(
        string name,
        long ownerId,
        Guid? excludePublicId = null,
        CancellationToken cancellationToken = default
    ) =>
        _context.Classes.AnyAsync(
            c =>
                c.Name == name
                && c.OwnerId == ownerId
                && (excludePublicId == null || c.PublicId != excludePublicId),
            cancellationToken
        );

    public Task<int> CountByOwnerAsync(
        long ownerId,
        CancellationToken cancellationToken = default
    ) => _context.Classes.CountAsync(c => c.OwnerId == ownerId, cancellationToken);

    public async Task AddAsync(Class entity, CancellationToken cancellationToken = default) =>
        await _context.Classes.AddAsync(entity, cancellationToken);

    public void Update(Class entity) => _context.Classes.Update(entity);
}

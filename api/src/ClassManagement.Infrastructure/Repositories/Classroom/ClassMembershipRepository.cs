using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;

// Class-membership data-access over the shared scoped DbContext. Mutation methods only STAGE changes
// — the handler commits through IUnitOfWork.SaveChangesAsync().
public sealed class ClassMembershipRepository : IClassMembershipRepository
{
    private readonly ApplicationDbContext _context;

    public ClassMembershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ClassMembership?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) =>
        _context.ClassMemberships.FirstOrDefaultAsync(
            m => m.PublicId == publicId,
            cancellationToken
        );

    public Task<ClassMembership?> GetActiveForStudentAsync(
        long classId,
        long studentId,
        CancellationToken cancellationToken = default
    ) =>
        _context.ClassMemberships.FirstOrDefaultAsync(
            m =>
                m.ClassId == classId
                && m.StudentId == studentId
                && (m.Status == MembershipStatus.Pending || m.Status == MembershipStatus.Approved),
            cancellationToken
        );

    public Task<bool> IsApprovedMemberAsync(
        long classId,
        long studentId,
        CancellationToken cancellationToken = default
    ) =>
        _context.ClassMemberships.AnyAsync(
            m =>
                m.ClassId == classId
                && m.StudentId == studentId
                && m.Status == MembershipStatus.Approved,
            cancellationToken
        );

    public Task<int> CountApprovedAsync(
        long classId,
        CancellationToken cancellationToken = default
    ) =>
        _context.ClassMemberships.CountAsync(
            m => m.ClassId == classId && m.Status == MembershipStatus.Approved,
            cancellationToken
        );

    public async Task AddAsync(
        ClassMembership entity,
        CancellationToken cancellationToken = default
    ) => await _context.ClassMemberships.AddAsync(entity, cancellationToken);

    public void Update(ClassMembership entity) => _context.ClassMemberships.Update(entity);
}

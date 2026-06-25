// Class-membership data-access over the shared scoped DbContext. Mutation methods only STAGE changes
// — the handler commits through IUnitOfWork.SaveChangesAsync().
public interface IClassMembershipRepository
{
    Task<ClassMembership?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    );

    /// <summary>The student's current active (Pending or Approved) membership in a class, if any.</summary>
    Task<ClassMembership?> GetActiveForStudentAsync(
        long classId,
        long studentId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsApprovedMemberAsync(
        long classId,
        long studentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Number of approved members in a class (for the max-students-per-class cap).</summary>
    Task<int> CountApprovedAsync(long classId, CancellationToken cancellationToken = default);

    Task AddAsync(ClassMembership entity, CancellationToken cancellationToken = default);

    void Update(ClassMembership entity);
}

// Attempt data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate them (including the owned Answers collection).
public interface IAttemptRepository
{
    Task<Attempt?> GetByPublicIdAsync(
        Guid publicId,
        bool includeAnswers = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>The student's current InProgress attempt for an assignment, if any (BR-5-06).</summary>
    Task<Attempt?> GetInProgressAsync(
        long assignmentId,
        long studentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>How many attempts the student already started for this assignment (for numbering + cap).</summary>
    Task<int> CountByStudentAsync(
        long assignmentId,
        long studentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>All InProgress attempts of an assignment, with answers loaded (for close → auto-submit).</summary>
    Task<IReadOnlyList<Attempt>> GetInProgressByAssignmentAsync(
        long assignmentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// A batch of InProgress attempts whose deadline has passed, with answers loaded (lifecycle
    /// auto-submit sweep). Bounded by <paramref name="batchSize"/> so one sweep can't run unbounded.
    /// </summary>
    Task<IReadOnlyList<Attempt>> GetExpiredInProgressAsync(
        DateTime asOf,
        int batchSize,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Attempt entity, CancellationToken cancellationToken = default);

    void Update(Attempt entity);
}

// Assignment data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate them (including the owned Snapshot aggregate).
public interface IAssignmentRepository
{
    // Loads an assignment by public id. When includeSnapshot is true, eager-loads the snapshot with
    // its questions + options (needed when reading correct answers for grading or when re-publishing
    // checks).
    Task<Assignment?> GetByPublicIdAsync(
        Guid publicId,
        bool includeSnapshot = false,
        CancellationToken cancellationToken = default
    );

    Task<int> CountByTeacherAsync(long teacherId, CancellationToken cancellationToken = default);

    // True if at least one attempt exists for the assignment (blocks delete).
    Task<bool> HasAttemptsAsync(long assignmentId, CancellationToken cancellationToken = default);

    /// <summary>Scheduled assignments whose opens_at has arrived (lifecycle Scheduled→Open sweep).</summary>
    Task<IReadOnlyList<Assignment>> GetScheduledDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    );

    /// <summary>Open assignments whose closes_at has passed (lifecycle Open→Closed sweep).</summary>
    Task<IReadOnlyList<Assignment>> GetOpenClosingDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Assignment entity, CancellationToken cancellationToken = default);

    void Update(Assignment entity);

    void Remove(Assignment entity);
}

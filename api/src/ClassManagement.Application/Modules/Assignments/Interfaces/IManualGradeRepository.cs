// Manual-grade data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers can
// upsert (update an existing grade or add a new one) in one unit of work.
public interface IManualGradeRepository
{
    /// <summary>All manual grades for an attempt (tracked, for upsert + total recompute).</summary>
    Task<IReadOnlyList<ManualGrade>> GetByAttemptAsync(
        long attemptId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(ManualGrade entity, CancellationToken cancellationToken = default);

    void Update(ManualGrade entity);
}

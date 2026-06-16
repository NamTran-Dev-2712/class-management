// Question data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate them (including the owned Options/Tags collections).
public interface IQuestionRepository
{
    // Loads a question by public id. When includeChildren is true, eager-loads Options + Tags so the
    // handler can map a detail DTO or replace the collections on update.
    Task<Question?> GetByPublicIdAsync(
        Guid publicId,
        bool includeChildren = false,
        CancellationToken cancellationToken = default
    );

    Task<int> CountByTeacherAsync(long teacherId, CancellationToken cancellationToken = default);

    Task AddAsync(Question entity, CancellationToken cancellationToken = default);

    void Update(Question entity);

    void Remove(Question entity);
}

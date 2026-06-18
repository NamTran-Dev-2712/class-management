// Exam data-access over the shared scoped DbContext. Mutation methods only STAGE changes — the
// handler commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities so handlers
// can mutate them (including the owned Questions collection).
public interface IExamRepository
{
    // Loads an exam by public id. When includeQuestions is true, eager-loads its ExamQuestion rows so
    // the handler can replace the collection on a question-list save.
    Task<Exam?> GetByPublicIdAsync(
        Guid publicId,
        bool includeQuestions = false,
        CancellationToken cancellationToken = default,
        bool includeTags = false
    );

    Task<int> CountByTeacherAsync(long teacherId, CancellationToken cancellationToken = default);

    Task AddAsync(Exam entity, CancellationToken cancellationToken = default);

    void Update(Exam entity);

    void Remove(Exam entity);
}

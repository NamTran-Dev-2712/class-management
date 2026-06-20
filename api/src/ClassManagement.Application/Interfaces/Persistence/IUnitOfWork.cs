public interface IUnitOfWork
{
    // Module repositories — handlers depend on IUnitOfWork only and reach the repos through it,
    // then commit via SaveChangesAsync. All repos share the unit's single DbContext.
    ISubjectRepository Subjects { get; }
    IClassRepository Classes { get; }
    IClassMembershipRepository ClassMemberships { get; }
    IQuestionRepository Questions { get; }
    IExamRepository Exams { get; }
    IAssignmentRepository Assignments { get; }
    IAttemptRepository Attempts { get; }
    IManualGradeRepository ManualGrades { get; }

    // Generic fallback for entities without a dedicated repository.
    IGenericRepository<T> Repository<T>()
        where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

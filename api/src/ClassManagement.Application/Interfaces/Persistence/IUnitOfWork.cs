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

    // Admin & moderation (MVP-7)
    IAuditLogRepository AuditLogs { get; }
    IReportRepository Reports { get; }
    INotificationRepository Notifications { get; }
    ISystemSettingRepository SystemSettings { get; }

    // Premium & payment (MVP-8)
    IPlanRepository Plans { get; }
    ISubscriptionRepository Subscriptions { get; }
    IPaymentRepository Payments { get; }
    IInvoiceRepository Invoices { get; }

    // Generic fallback for entities without a dedicated repository.
    IGenericRepository<T> Repository<T>()
        where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Runs <paramref name="operation"/> inside a single DB transaction, wrapped in the provider's
    // execution strategy so it is safe with EnableRetryOnFailure (manual BeginTransaction is not).
    // Use for multi-SaveChanges atomic flows (e.g. the payment webhook).
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    );

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

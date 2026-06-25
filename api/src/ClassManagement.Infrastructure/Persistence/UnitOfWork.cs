using ClassManagement.Infrastructure.Persistence.DbContext;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Repositories are constructor-injected and share this unit's scoped DbContext (both resolve
    // the same pooled instance per request), so their staged changes commit on SaveChangesAsync.
    public UnitOfWork(
        ApplicationDbContext context,
        ISubjectRepository subjects,
        IClassRepository classes,
        IClassMembershipRepository classMemberships,
        IQuestionRepository questions,
        IExamRepository exams,
        IAssignmentRepository assignments,
        IAttemptRepository attempts,
        IManualGradeRepository manualGrades,
        IAuditLogRepository auditLogs,
        IReportRepository reports,
        INotificationRepository notifications,
        ISystemSettingRepository systemSettings
    )
    {
        _context = context;
        Subjects = subjects;
        Classes = classes;
        ClassMemberships = classMemberships;
        Questions = questions;
        Exams = exams;
        Assignments = assignments;
        Attempts = attempts;
        ManualGrades = manualGrades;
        AuditLogs = auditLogs;
        Reports = reports;
        Notifications = notifications;
        SystemSettings = systemSettings;
    }

    public ISubjectRepository Subjects { get; }
    public IClassRepository Classes { get; }
    public IClassMembershipRepository ClassMemberships { get; }
    public IQuestionRepository Questions { get; }
    public IExamRepository Exams { get; }
    public IAssignmentRepository Assignments { get; }
    public IAttemptRepository Attempts { get; }
    public IManualGradeRepository ManualGrades { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IReportRepository Reports { get; }
    public INotificationRepository Notifications { get; }
    public ISystemSettingRepository SystemSettings { get; }

    public IGenericRepository<T> Repository<T>()
        where T : class
    {
        return new GenericRepository<T>(_context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }
}

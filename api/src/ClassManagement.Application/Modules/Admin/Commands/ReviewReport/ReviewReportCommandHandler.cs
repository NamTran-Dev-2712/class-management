using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

public class ReviewReportCommandHandler : IRequestHandler<ReviewReportCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IAuditLogger _auditLogger;
    private readonly IUserAdminRepository _userAdmin;

    public ReviewReportCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IAuditLogger auditLogger,
        IUserAdminRepository userAdmin
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
        _auditLogger = auditLogger;
        _userAdmin = userAdmin;
    }

    public async Task Handle(ReviewReportCommand request, CancellationToken ct)
    {
        var adminId = _currentUser.UserId;

        var report =
            await _unitOfWork.Reports.GetByPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Report.NotFound");

        // A closed report is final.
        if (report.Status is ReportStatus.Resolved or ReportStatus.Rejected)
            throw new BadException("Report.AlreadyReviewed");

        report.AdminId = adminId;
        report.AdminNote = request.AdminNote?.Trim();

        if (request.Status == ReportStatus.Reviewing)
        {
            report.Status = ReportStatus.Reviewing;
            _unitOfWork.Reports.Update(report);
            await _auditLogger.LogAsync(BuildAudit(report, AuditActions.ReportReviewed), ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        // Terminal decision (Resolved / Rejected).
        report.Status = request.Status;
        report.AdminAction = request.AdminAction;
        report.ResolvedAt = DateTime.UtcNow;
        _unitOfWork.Reports.Update(report);

        if (request.Status == ReportStatus.Resolved && request.AdminAction is { } action)
            await ApplyActionAsync(report, action, adminId, ct);

        await _auditLogger.LogAsync(
            BuildAudit(
                report,
                request.Status == ReportStatus.Resolved
                    ? AuditActions.ReportResolved
                    : AuditActions.ReportRejected
            ),
            ct
        );

        // Tell the reporter their report was handled.
        await _notifications.NotifyAsync(
            report.ReporterId,
            new NotificationContent
            {
                EventType = NotificationEventType.ReportResolved,
                Title = "Report resolved",
                Body = "Your report has been reviewed.",
                Link = "/",
                ReferenceId = report.PublicId.ToString(),
                Payload = new Dictionary<string, object?>
                {
                    ["status"] = report.Status.ToString(),
                    ["action"] = report.AdminAction?.ToString(),
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyActionAsync(
        Report report,
        ReportAdminAction action,
        long? adminId,
        CancellationToken ct
    )
    {
        switch (action)
        {
            case ReportAdminAction.Dismiss:
                break;

            case ReportAdminAction.WarnUser:
                await WarnSubjectAsync(report, ct);
                break;

            case ReportAdminAction.HideContent:
                await SoftDeleteContentAsync(report, ct);
                break;

            case ReportAdminAction.DeleteContent:
                await DeleteContentAsync(report, ct);
                break;

            case ReportAdminAction.BanUser:
                await BanUserAsync(report, adminId, ct);
                break;
        }
    }

    private async Task WarnSubjectAsync(Report report, CancellationToken ct)
    {
        var subjectId = await ReportTargetResolver.ResolveSubjectUserIdAsync(
            _unitOfWork,
            report.TargetType,
            report.TargetId,
            ct
        );
        if (subjectId is null)
            return;

        await _notifications.NotifyAsync(
            subjectId.Value,
            new NotificationContent
            {
                EventType = NotificationEventType.ReportResolved,
                Title = "Warning",
                Body = "An administrator has issued a warning regarding your content or activity.",
                Link = "/",
                ReferenceId = $"warn:{report.PublicId}",
                Payload = new Dictionary<string, object?> { ["kind"] = "warning" },
            },
            ct
        );
    }

    private async Task BanUserAsync(Report report, long? adminId, CancellationToken ct)
    {
        if (report.TargetType != ReportTargetType.User || report.TargetPublicId is null)
            throw new BadException("Report.BanRequiresUserTarget");

        // Locks the account + revokes all refresh tokens (BR-7-03). Persists via UserManager itself.
        await _userAdmin.SetLockAsync(report.TargetPublicId.Value, true, adminId, ct);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.UserLocked,
                TargetType = AuditTargetTypes.User,
                TargetPublicId = report.TargetPublicId,
                Metadata = new Dictionary<string, object?> { ["reason"] = "report_ban" },
            },
            ct
        );
    }

    // Hide = soft-delete the content (kept in DB, gone from every read). Used by HideContent.
    private async Task SoftDeleteContentAsync(Report report, CancellationToken ct)
    {
        switch (report.TargetType)
        {
            case ReportTargetType.Question:
                await RemoveByIdAsync<Question>(report.TargetId, ct);
                break;
            case ReportTargetType.Exam:
                await RemoveByIdAsync<Exam>(report.TargetId, ct);
                break;
            case ReportTargetType.Assignment:
                await RemoveByIdAsync<Assignment>(report.TargetId, ct);
                break;
            case ReportTargetType.Class:
                await RemoveByIdAsync<Class>(report.TargetId, ct);
                break;
            default:
                throw new BadException("Report.ContentActionNotApplicable");
        }
    }

    // Delete blocks a Question still referenced by an exam (BR-7-05) — admin must Hide instead.
    private async Task DeleteContentAsync(Report report, CancellationToken ct)
    {
        if (report.TargetType == ReportTargetType.Question)
        {
            var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();
            var inUse = await examQuestionRepo.ExistsAsync(eq => eq.QuestionId == report.TargetId);
            if (inUse)
                throw new ConflictException("Report.DeleteBlockedInUse");
        }

        await SoftDeleteContentAsync(report, ct);
    }

    private async Task RemoveByIdAsync<TEntity>(long id, CancellationToken ct)
        where TEntity : class
    {
        var repo = _unitOfWork.Repository<TEntity>();
        var entity = await repo.GetFirstOrDefaultAsync(BuildIdPredicate<TEntity>(id));
        if (entity is not null)
            repo.Remove(entity);
    }

    private static System.Linq.Expressions.Expression<
        Func<TEntity, bool>
    > BuildIdPredicate<TEntity>(long id)
    {
        var p = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "e");
        var body = System.Linq.Expressions.Expression.Equal(
            System.Linq.Expressions.Expression.Property(p, nameof(BaseEntity.Id)),
            System.Linq.Expressions.Expression.Constant(id)
        );
        return System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(body, p);
    }

    private AuditEntry BuildAudit(Report report, string action) =>
        new()
        {
            Action = action,
            TargetType = AuditTargetTypes.Report,
            TargetPublicId = report.PublicId,
            Metadata = new Dictionary<string, object?>
            {
                ["targetType"] = report.TargetType.ToString(),
                ["adminAction"] = report.AdminAction?.ToString(),
            },
        };
}

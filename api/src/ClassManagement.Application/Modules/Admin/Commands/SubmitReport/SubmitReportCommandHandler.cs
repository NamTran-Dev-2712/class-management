using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

public class SubmitReportCommandHandler : IRequestHandler<SubmitReportCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserDirectory _userDirectory;
    private readonly INotificationService _notifications;
    private readonly IAuditLogger _auditLogger;
    private readonly ISystemSettingsService _settings;

    public SubmitReportCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IUserDirectory userDirectory,
        INotificationService notifications,
        IAuditLogger auditLogger,
        ISystemSettingsService settings
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _userDirectory = userDirectory;
        _notifications = notifications;
        _auditLogger = auditLogger;
        _settings = settings;
    }

    public async Task<Guid> Handle(SubmitReportCommand request, CancellationToken ct)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");

        // Anti-spam daily cap (risk mitigation).
        var maxPerDay = await _settings.GetIntAsync(SystemSettingKeys.MaxReportsPerDay, 10, ct);
        var since = DateTime.UtcNow.AddDays(-1);
        var recentCount = await _unitOfWork.Reports.CountByReporterSinceAsync(userId, since, ct);
        if (recentCount >= maxPerDay)
            throw new BadException("Report.DailyLimitReached");

        var targetId =
            await ReportTargetResolver.ResolveAsync(
                _unitOfWork,
                _userDirectory,
                request.TargetType,
                request.TargetPublicId,
                ct
            ) ?? throw new NotFoundException("Report.TargetNotFound");

        // Idempotent: an existing active (Pending/Reviewing) report for this target by this user wins.
        var existing = await _unitOfWork.Reports.FindActiveAsync(
            userId,
            request.TargetType,
            targetId,
            ct
        );
        if (existing is not null)
            return existing.PublicId;

        var report = new Report
        {
            ReporterId = userId,
            TargetType = request.TargetType,
            TargetId = targetId,
            TargetPublicId = request.TargetPublicId,
            Reason = request.Reason,
            Description = request.Description?.Trim(),
            Status = ReportStatus.Pending,
        };
        await _unitOfWork.Reports.AddAsync(report, ct);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.ReportCreated,
                TargetType = AuditTargetTypes.Report,
                TargetPublicId = request.TargetPublicId,
                Metadata = new Dictionary<string, object?>
                {
                    ["targetType"] = request.TargetType.ToString(),
                    ["reason"] = request.Reason.ToString(),
                },
            },
            ct
        );

        // Notify admins (one per reported target, deduped) that a report needs review.
        var adminIds = await _userDirectory.GetActiveUserIdsAsync(ApplicationRoles.Admin, ct);
        await _notifications.NotifyManyAsync(
            adminIds,
            new NotificationContent
            {
                EventType = NotificationEventType.ReportReceived,
                Title = "New report",
                Body = $"A {request.TargetType} was reported and needs review.",
                Link = "/admin/reports",
                ReferenceId = $"{request.TargetType}:{targetId}",
                Payload = new Dictionary<string, object?>
                {
                    ["targetType"] = request.TargetType.ToString(),
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
        return report.PublicId;
    }
}

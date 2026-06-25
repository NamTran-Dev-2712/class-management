using ClassManagement.Domain.Modules.Admin.Constants;

// System job (MVP-7): archive Read notifications older than 30 days, then permanently delete Archived
// ones older than the configured retention window. Set-based; safe to re-run.
public record CleanupNotificationsCommand : IRequest;

public class CleanupNotificationsCommandHandler : IRequestHandler<CleanupNotificationsCommand>
{
    private const int ArchiveAfterDays = 30;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemSettingsService _settings;

    public CleanupNotificationsCommandHandler(
        IUnitOfWork unitOfWork,
        ISystemSettingsService settings
    )
    {
        _unitOfWork = unitOfWork;
        _settings = settings;
    }

    public async Task Handle(CleanupNotificationsCommand request, CancellationToken ct)
    {
        var retainDays = await _settings.GetIntAsync(
            SystemSettingKeys.NotificationCleanupDays,
            90,
            ct
        );
        var now = DateTime.UtcNow;

        await _unitOfWork.Notifications.ArchiveReadOlderThanAsync(
            now.AddDays(-ArchiveAfterDays),
            ct
        );
        await _unitOfWork.Notifications.DeleteArchivedOlderThanAsync(now.AddDays(-retainDays), ct);
    }
}

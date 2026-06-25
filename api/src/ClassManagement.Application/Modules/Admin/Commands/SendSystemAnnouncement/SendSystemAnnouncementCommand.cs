using ClassManagement.Application.Common.Constants;
using ClassManagement.Domain.Modules.Admin.Enums;

// Admin sends a system announcement (A7-08) to all active users or a specific role. Returns the number
// of recipients. Fan-out is a single batch insert (one transaction) so it does not depend on Hangfire.
public record SendSystemAnnouncementCommand(string Title, string? Body, string? Role)
    : IRequest<int>;

public class SendSystemAnnouncementCommandHandler
    : IRequestHandler<SendSystemAnnouncementCommand, int>
{
    private readonly IUserDirectory _userDirectory;
    private readonly INotificationService _notifications;

    public SendSystemAnnouncementCommandHandler(
        IUserDirectory userDirectory,
        INotificationService notifications
    )
    {
        _userDirectory = userDirectory;
        _notifications = notifications;
    }

    public async Task<int> Handle(SendSystemAnnouncementCommand request, CancellationToken ct)
    {
        var role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim();
        var recipients = await _userDirectory.GetActiveUserIdsAsync(role, ct);
        if (recipients.Count == 0)
            return 0;

        await _notifications.NotifyAndSaveAsync(
            recipients,
            new NotificationContent
            {
                EventType = NotificationEventType.SystemAnnouncement,
                Title = request.Title.Trim(),
                Body = request.Body?.Trim(),
                Payload = new Dictionary<string, object?>
                {
                    ["title"] = request.Title.Trim(),
                    ["body"] = request.Body?.Trim(),
                },
            },
            ct
        );

        return recipients.Count;
    }
}

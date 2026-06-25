using ClassManagement.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace ClassManagement.Api.Hubs;

// IRealtimeNotifier over SignalR (MVP-7.5). Pushes "notificationsChanged" to each target user via the
// default user identifier (NameIdentifier claim = user id). No-op when a user has no live connection.
public sealed class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public Task NotificationsChangedAsync(
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default
    )
    {
        if (userIds.Count == 0)
            return Task.CompletedTask;

        var ids = userIds.Select(id => id.ToString()).ToList();
        return _hub.Clients.Users(ids).SendAsync("notificationsChanged", cancellationToken);
    }
}

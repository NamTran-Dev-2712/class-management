namespace ClassManagement.Application.Interfaces.Notifications;

/// <summary>
/// Pushes a lightweight realtime signal to users (MVP-7.5). Implemented in the API layer over SignalR;
/// Application stays transport-agnostic. Best-effort — the persisted notification + polling fallback are
/// the source of truth, so a missed push is harmless.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Tell each user their notifications changed (client refetches unread-count + list).</summary>
    Task NotificationsChangedAsync(
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default
    );
}

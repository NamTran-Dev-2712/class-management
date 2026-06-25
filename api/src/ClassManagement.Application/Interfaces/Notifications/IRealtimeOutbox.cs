namespace ClassManagement.Application.Interfaces.Notifications;

/// <summary>
/// A per-request buffer of users whose notifications changed (MVP-7.5). The notification service queues
/// recipients as it stages rows; <c>RealtimeDispatchBehavior</c> drains and flushes them via
/// <see cref="IRealtimeNotifier"/> only after the request handler succeeds — so realtime pushes never
/// reference a notification that was rolled back. Scoped per request.
/// </summary>
public interface IRealtimeOutbox
{
    void QueueNotificationsChanged(IEnumerable<long> userIds);

    /// <summary>Returns and clears the buffered recipient ids.</summary>
    IReadOnlyCollection<long> Drain();
}

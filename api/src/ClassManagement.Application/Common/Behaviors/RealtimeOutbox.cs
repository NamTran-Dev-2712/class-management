namespace ClassManagement.Application.Common.Behaviors;

// Scoped in-memory implementation of IRealtimeOutbox — a HashSet of recipient ids buffered during a
// request and drained once by RealtimeDispatchBehavior after the handler succeeds.
public sealed class RealtimeOutbox : IRealtimeOutbox
{
    private readonly HashSet<long> _userIds = [];

    public void QueueNotificationsChanged(IEnumerable<long> userIds)
    {
        foreach (var id in userIds)
            _userIds.Add(id);
    }

    public IReadOnlyCollection<long> Drain()
    {
        if (_userIds.Count == 0)
            return [];
        var snapshot = _userIds.ToArray();
        _userIds.Clear();
        return snapshot;
    }
}

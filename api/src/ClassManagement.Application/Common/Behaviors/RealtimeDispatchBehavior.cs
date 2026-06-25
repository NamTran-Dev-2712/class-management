namespace ClassManagement.Application.Common.Behaviors;

// Flushes the realtime outbox AFTER the inner handler succeeds (MVP-7.5). Registered outermost so it
// wraps validation/audit/handler: if anything throws, the push is skipped (no phantom realtime events).
// Best-effort — a realtime failure must never fail the request, so the flush is fire-and-protected.
public sealed class RealtimeDispatchBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IRealtimeOutbox _outbox;
    private readonly IRealtimeNotifier _notifier;

    public RealtimeDispatchBehavior(IRealtimeOutbox outbox, IRealtimeNotifier notifier)
    {
        _outbox = outbox;
        _notifier = notifier;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next(cancellationToken);

        var recipients = _outbox.Drain();
        if (recipients.Count > 0)
        {
            try
            {
                await _notifier.NotificationsChangedAsync(recipients, cancellationToken);
            }
            catch
            {
                // Realtime is best-effort; the persisted notification + polling fallback still deliver.
            }
        }

        return response;
    }
}

using Hangfire;
using MediatR;

namespace ClassManagement.Infrastructure.Services.Payment.Jobs;

// Thin Hangfire trigger for the subscription lifecycle sweep (MVP-8). All logic lives in the Application
// RunSubscriptionLifecycleCommand handler, which is idempotent — a missed tick is picked up next run.
public sealed class SubscriptionLifecycleJob
{
    private readonly ISender _mediator;

    public SubscriptionLifecycleJob(ISender mediator)
    {
        _mediator = mediator;
    }

    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync() => _mediator.Send(new RunSubscriptionLifecycleCommand());
}

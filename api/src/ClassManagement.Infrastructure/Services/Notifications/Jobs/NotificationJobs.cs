using Hangfire;
using MediatR;

namespace ClassManagement.Infrastructure.Services.Notifications.Jobs;

// Thin Hangfire triggers for the notification background work (MVP-7). All logic lives in the Application
// command handlers; dependencies are resolved per-invocation from a fresh DI scope.

public sealed class AssignmentDueSoonJob
{
    private readonly ISender _mediator;

    public AssignmentDueSoonJob(ISender mediator)
    {
        _mediator = mediator;
    }

    // Idempotent (reference-id dedup), so a missed tick is simply caught next run — no retry/pile-up.
    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync() => _mediator.Send(new SendAssignmentDueSoonRemindersCommand());
}

public sealed class NotificationCleanupJob
{
    private readonly ISender _mediator;

    public NotificationCleanupJob(ISender mediator)
    {
        _mediator = mediator;
    }

    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync() => _mediator.Send(new CleanupNotificationsCommand());
}

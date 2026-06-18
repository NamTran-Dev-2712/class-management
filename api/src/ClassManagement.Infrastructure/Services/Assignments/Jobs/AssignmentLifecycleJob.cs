using Hangfire;
using MediatR;

namespace ClassManagement.Infrastructure.Services.Assignments.Jobs;

// Thin Hangfire trigger for the assignment/attempt lifecycle sweep. All logic lives in the Application
// RunAssignmentLifecycleCommand handler (so the system and student paths share the same finalize +
// auto-grade code). Dependencies are resolved per-invocation from a fresh DI scope.
public sealed class AssignmentLifecycleJob
{
    private readonly ISender _mediator;

    public AssignmentLifecycleJob(ISender mediator)
    {
        _mediator = mediator;
    }

    // No automatic retry: the sweep runs frequently and is idempotent, so a missed tick is simply
    // picked up on the next run rather than retried (avoids overlap/pile-up).
    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync() => _mediator.Send(new RunAssignmentLifecycleCommand());
}

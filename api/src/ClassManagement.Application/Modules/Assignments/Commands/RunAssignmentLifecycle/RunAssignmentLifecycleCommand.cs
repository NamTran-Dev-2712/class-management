// System (background-job) command that advances assignment/attempt state machines: Scheduled→Open,
// Open→Closed (auto-submitting their InProgress attempts), and auto-submit of attempts past their
// deadline (BR-5-07/08). No current user — runs as the system. Idempotent + bounded per sweep.
public record RunAssignmentLifecycleCommand : IRequest;

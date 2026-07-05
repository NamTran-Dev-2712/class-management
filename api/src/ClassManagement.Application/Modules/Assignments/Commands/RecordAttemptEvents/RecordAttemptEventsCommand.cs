using ClassManagement.Application.Modules.Assignments.DTOs;

// Student's browser reports a batch of proctoring signals for an in-progress attempt (MVP-10). Reuses the
// SaveAttemptAnswers guard chain (owner + InProgress + not-past-deadline) plus an is_locked reject. The
// server increments violation_count and, when it exceeds MaxViolations (> 0), applies ViolationAction.
// AttemptId comes from the route.
public record RecordAttemptEventsCommand(List<AttemptEventInput> Events)
    : IRequest<RecordEventsResultDto>
{
    public Guid AttemptId { get; init; }
}

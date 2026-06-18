// Teacher publishes a Draft assignment: builds the immutable snapshot (copy of the exam's questions +
// options at this moment) and moves the assignment to Scheduled (opens later) or Open (opens now).
// Idempotent — rejected if already published. PublicId comes from the route.
public record PublishAssignmentCommand(Guid PublicId) : IRequest;

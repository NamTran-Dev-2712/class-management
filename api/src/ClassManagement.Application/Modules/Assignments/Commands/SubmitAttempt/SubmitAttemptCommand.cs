// Student submits an attempt. Idempotent — rejected if not InProgress (guards the race with server
// auto-submit). If the server deadline has already passed, the submission is recorded as
// auto_submitted. AttemptId comes from the route.
public record SubmitAttemptCommand(Guid AttemptId) : IRequest;

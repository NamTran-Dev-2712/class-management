// Teacher deletes an assignment (soft-delete). Blocked if any attempt exists (preserve history).
// PublicId comes from the route.
public record DeleteAssignmentCommand(Guid PublicId) : IRequest;

// Teacher archives an assignment (any state → Archived). Archived assignments are hidden from active
// lists; attempts are preserved. PublicId comes from the route.
public record ArchiveAssignmentCommand(Guid PublicId) : IRequest;

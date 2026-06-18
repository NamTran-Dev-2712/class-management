// Teacher closes an Open assignment early (Open→Closed). All InProgress attempts are auto-submitted
// immediately (BR-5-08). PublicId comes from the route.
public record CloseAssignmentCommand(Guid PublicId) : IRequest;

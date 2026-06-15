// Student voluntarily leaves a class they are an approved member of. Past data is preserved
// (BR-2-07) — the membership transitions to Left.
public record LeaveClassCommand(Guid ClassPublicId) : IRequest;

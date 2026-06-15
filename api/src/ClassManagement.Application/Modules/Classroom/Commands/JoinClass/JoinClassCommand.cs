// Student requests to join a class by its invite code. Creates a Pending membership. Returns the
// class PublicId so the client can navigate.
public record JoinClassCommand(string InviteCode) : IRequest<Guid>;

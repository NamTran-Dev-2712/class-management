// Teacher approves a pending join request in an owned class.
public record ApproveMemberCommand(Guid ClassPublicId, Guid MembershipPublicId) : IRequest;

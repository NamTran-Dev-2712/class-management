// Teacher removes an approved student from an owned class. The student's past data is preserved
// (BR-2-06) — only the membership transitions to Removed.
public record KickMemberCommand(Guid ClassPublicId, Guid MembershipPublicId) : IRequest;

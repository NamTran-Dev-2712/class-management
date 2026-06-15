// Teacher rejects a pending join request in an owned class. The student may re-join later with the
// invite code (a new membership row).
public record RejectMemberCommand(
    Guid ClassPublicId,
    Guid MembershipPublicId,
    string? RejectionReason
) : IRequest;

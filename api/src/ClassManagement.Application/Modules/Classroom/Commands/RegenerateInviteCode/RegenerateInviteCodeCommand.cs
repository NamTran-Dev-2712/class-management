// Teacher regenerates an owned class's invite code. The old code is invalidated immediately
// (BR-2-02). Returns the new code.
public record RegenerateInviteCodeCommand(Guid PublicId) : IRequest<string>;

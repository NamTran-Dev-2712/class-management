// Generates a class invite code: crypto-random [A-Z0-9] of the given length (live-configurable via
// IClassroomPolicy.GetInviteCodeLengthAsync). Uniqueness is ensured by the handler retrying against
// IClassRepository.InviteCodeExistsAsync.
public interface IInviteCodeGenerator
{
    string Generate(int length);
}

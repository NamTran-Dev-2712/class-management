// Generates a class invite code: 8-char crypto-random [A-Z0-9]. Uniqueness is ensured by the handler
// retrying against IClassRepository.InviteCodeExistsAsync.
public interface IInviteCodeGenerator
{
    string Generate();
}

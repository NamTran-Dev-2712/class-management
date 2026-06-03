using ClassManagement.Application.Common.Constants;

public interface IAuthRepository
{
    Task<AuthResult> LoginAsync(string email, string password);

    Task<long> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role = ApplicationRoles.Student,
        CancellationToken cancellationToken = default
    );
}

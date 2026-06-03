using ClassManagement.Application.Common.Constants;

public interface IAuthRepository
{
    Task<long> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role = ApplicationRoles.Student,
        CancellationToken cancellationToken = default
    );
}

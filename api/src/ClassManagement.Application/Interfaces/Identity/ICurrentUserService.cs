namespace ClassManagement.Application.Interfaces.Identity;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

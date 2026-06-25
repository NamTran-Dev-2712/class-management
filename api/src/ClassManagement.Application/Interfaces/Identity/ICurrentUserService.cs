namespace ClassManagement.Application.Interfaces.Identity;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    /// <summary>The single role the current request is acting in (highest privilege), or null.</summary>
    string? Role { get; }

    /// <summary>Caller IP address (for audit logging), or null outside an HTTP request.</summary>
    string? IpAddress { get; }

    /// <summary>Caller User-Agent header (for audit logging), or null.</summary>
    string? UserAgent { get; }
}

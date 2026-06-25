using System.Security.Claims;
using ClassManagement.Application.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace ClassManagement.Infrastructure.Services.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    // Highest-privilege role the caller holds (Admin > Teacher > Student) — used for audit actor_role.
    public string? Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
                return null;
            if (user.IsInRole(ApplicationRoles.Admin))
                return ApplicationRoles.Admin;
            if (user.IsInRole(ApplicationRoles.Teacher))
                return ApplicationRoles.Teacher;
            if (user.IsInRole(ApplicationRoles.Student))
                return ApplicationRoles.Student;
            return null;
        }
    }

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua
            ? ua
            : null;
}

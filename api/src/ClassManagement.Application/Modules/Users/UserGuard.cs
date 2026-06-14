using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

// Handler-side guard for admin user management. The Users controller is Admin-only, so the caller
// is always an admin — rejecting admin targets therefore also prevents managing yourself.
internal static class UserGuard
{
    public static async Task<UserDetailDto> EnsureManageableAsync(
        IUserAdminRepository users,
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var target =
            await users.GetByPublicIdAsync(publicId, cancellationToken)
            ?? throw new NotFoundException("User.NotFound");

        // Admins (including the current admin) are out of management scope.
        if (target.Roles.Contains(ApplicationRoles.Admin))
            throw new ForbiddenException("User.CannotManageAdmin");

        return target;
    }
}

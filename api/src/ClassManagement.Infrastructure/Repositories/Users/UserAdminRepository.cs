using ClassManagement.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

// Thin data-access over ASP.NET Identity. Keeps UserManager/ApplicationUser confined to
// Infrastructure — Application talks to this through IUserAdminRepository only. Business rules
// (duplicate email, admin/self guards, role choice) live in the handlers. List reads use the
// vw_admin_users view (see GetUsersQueryHandler).
public sealed class UserAdminRepository : IUserAdminRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenRepository _refreshTokens;

    public UserAdminRepository(
        UserManager<ApplicationUser> userManager,
        IRefreshTokenRepository refreshTokens
    )
    {
        _userManager = userManager;
        _refreshTokens = refreshTokens;
    }

    public async Task<UserDetailDto?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.PublicId == publicId, cancellationToken);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return ToDetailDto(user, roles);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => await _userManager.FindByEmailAsync(email) is not null;

    public async Task<Guid> CreateAsync(
        string displayName,
        string email,
        string role,
        string? phoneNumber,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        var user = new ApplicationUser
        {
            DisplayName = displayName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            // Admin-created accounts are vouched-for; no email confirmation step needed.
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new BadException("User.CreateFailed");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new BadException("User.RoleAssignFailed");
        }

        return user.PublicId;
    }

    public async Task UpdateAsync(
        Guid publicId,
        string displayName,
        string role,
        string? phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        var user = await FindTrackedAsync(publicId, cancellationToken);

        user.DisplayName = displayName;
        user.PhoneNumber = phoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new BadException("User.UpdateFailed");

        await SwapRoleAsync(user, role);
    }

    public async Task SetLockAsync(
        Guid publicId,
        bool locked,
        long? byUserId,
        CancellationToken cancellationToken = default
    )
    {
        var user = await FindTrackedAsync(publicId, cancellationToken);

        user.IsLocked = locked;
        user.LockedAt = locked ? DateTime.UtcNow : null;
        user.LockedBy = locked ? byUserId : null;
        await _userManager.UpdateAsync(user);

        // Force sign-out everywhere — a locked account must not keep a live session.
        if (locked)
            await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        var user = await FindTrackedAsync(publicId, cancellationToken);

        // Soft delete — the partial unique email index (deleted_at IS NULL) frees the email.
        user.DeletedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
    }

    private async Task<ApplicationUser> FindTrackedAsync(
        Guid publicId,
        CancellationToken cancellationToken
    ) =>
        await _userManager.Users.FirstOrDefaultAsync(u => u.PublicId == publicId, cancellationToken)
        ?? throw new NotFoundException("User.NotFound");

    // Single-role model: ensure the user ends up with exactly the requested role.
    private async Task SwapRoleAsync(ApplicationUser user, string role)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count == 1 && currentRoles[0] == role)
            return;

        if (currentRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
            throw new BadException("User.RoleAssignFailed");
    }

    private static UserDetailDto ToDetailDto(ApplicationUser user, IList<string> roles) =>
        new()
        {
            PublicId = user.PublicId,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LockedAt = user.LockedAt,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = [.. roles],
        };
}

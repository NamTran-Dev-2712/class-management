using ClassManagement.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Infrastructure.Services.Identity;

// Resolves active user ids (optionally by role) over UserManager for announcement fan-out (MVP-7).
public sealed class UserDirectory : IUserDirectory
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDirectory(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<long>> GetActiveUserIdsAsync(
        string? role = null,
        CancellationToken cancellationToken = default
    )
    {
        // IsDeleted is a computed property (DeletedAt.HasValue) and is not SQL-translatable — filter on
        // the mapped DeletedAt column directly.
        if (string.IsNullOrWhiteSpace(role))
            return await _userManager
                .Users.Where(u => u.DeletedAt == null && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

        var inRole = await _userManager.GetUsersInRoleAsync(role);
        return inRole.Where(u => !u.IsDeleted && u.IsActive).Select(u => u.Id).ToList();
    }

    public async Task<long?> GetUserIdByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    )
    {
        var match = await _userManager
            .Users.Where(u => u.PublicId == publicId)
            .Select(u => (long?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return match;
    }

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        _userManager.Users.Where(u => u.DeletedAt == null).CountAsync(cancellationToken);

    public Task<int> CountUsersCreatedSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default
    ) =>
        _userManager
            .Users.Where(u => u.DeletedAt == null && u.CreatedAt >= since)
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<DateOnly, int>> GetDailyNewUserCountsAsync(
        DateTime since,
        CancellationToken cancellationToken = default
    )
    {
        // Group by the registration date (Npgsql translates DateTime.Date to a date cast).
        var rows = await _userManager
            .Users.Where(u => u.DeletedAt == null && u.CreatedAt >= since)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => DateOnly.FromDateTime(r.Day), r => r.Count);
    }
}

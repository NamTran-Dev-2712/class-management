namespace ClassManagement.Application.Interfaces.Identity;

/// <summary>
/// Read-only access to the set of active user ids, optionally scoped to a role (MVP-7). Used to fan out
/// system announcements without leaking Identity types into Application. Excludes deleted/disabled users.
/// </summary>
public interface IUserDirectory
{
    Task<IReadOnlyList<long>> GetActiveUserIdsAsync(
        string? role = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>Resolve a user's internal id from its public id, or null if not found.</summary>
    Task<long?> GetUserIdByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Total non-deleted users (admin dashboard).</summary>
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>Non-deleted users created on/after the cutoff (admin dashboard).</summary>
    Task<int> CountUsersCreatedSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default
    );

    /// <summary>Per-day new-user counts since the cutoff, keyed by date (admin dashboard chart).</summary>
    Task<IReadOnlyDictionary<DateOnly, int>> GetDailyNewUserCountsAsync(
        DateTime since,
        CancellationToken cancellationToken = default
    );
}

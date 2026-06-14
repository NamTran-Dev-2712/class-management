// Thin data-access over ASP.NET Identity for admin user management. Keeps
// UserManager/ApplicationUser confined to Infrastructure — Application talks to this through the
// interface only. All business rules (duplicate email, admin/self guards, role choice) live in the
// command/query handlers; this type just reads and writes. List reads use the vw_admin_users view.
public interface IUserAdminRepository
{
    Task<UserDetailDto?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    );

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    // Creates the account with the given (handler-generated) password and assigns the role.
    // Returns the new user's PublicId.
    Task<Guid> CreateAsync(
        string displayName,
        string email,
        string role,
        string? phoneNumber,
        string password,
        CancellationToken cancellationToken = default
    );

    // Updates editable profile fields (email is immutable) and swaps the single role.
    Task UpdateAsync(
        Guid publicId,
        string displayName,
        string role,
        string? phoneNumber,
        CancellationToken cancellationToken = default
    );

    // Sets the lock state (recording who/when) and revokes all sessions when locking.
    Task SetLockAsync(
        Guid publicId,
        bool locked,
        long? byUserId,
        CancellationToken cancellationToken = default
    );

    // Soft-deletes the account and revokes all sessions.
    Task SoftDeleteAsync(Guid publicId, CancellationToken cancellationToken = default);
}

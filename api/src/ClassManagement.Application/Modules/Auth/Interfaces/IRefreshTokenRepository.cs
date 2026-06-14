public interface IRefreshTokenRepository
{
    Task UpdateRefreshTokenAsync(
        long userId,
        string tokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    );

    Task<RefreshToken?> FindActiveByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default
    );

    Task RotateAsync(
        long oldTokenId,
        long userId,
        string newTokenHash,
        DateTime newExpiresAt,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default
    );

    Task RevokeByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(long userId, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository
{
    Task UpdateRefreshTokenAsync(
        long userId,
        string tokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    );
}

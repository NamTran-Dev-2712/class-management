public interface IPasswordResetTokenRepository
{
    // Marks any existing un-used token for the user as used, so only the latest OTP is valid.
    Task InvalidatePreviousAsync(long userId, CancellationToken cancellationToken = default);

    Task CreateAsync(
        long userId,
        string tokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    );

    // Newest un-used token for the user (expiry checked by caller against the entity).
    Task<PasswordResetToken?> FindActiveByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default
    );

    Task IncrementAttemptAsync(long tokenId, CancellationToken cancellationToken = default);

    Task MarkUsedAsync(long tokenId, CancellationToken cancellationToken = default);
}

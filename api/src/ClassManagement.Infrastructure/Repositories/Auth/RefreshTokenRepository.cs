using ClassManagement.Infrastructure.Persistence.DbContext;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateRefreshTokenAsync(
        long userId,
        string tokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    )
    {
        // Revoke all active tokens for this user (single-session policy)
        await _context
            .RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow),
                cancellationToken
            );

        _context.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
            }
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> FindActiveByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default
    ) =>
        _context.RefreshTokens.FirstOrDefaultAsync(
            rt =>
                rt.TokenHash == tokenHash && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow,
            cancellationToken
        );

    public async Task RotateAsync(
        long oldTokenId,
        long userId,
        string newTokenHash,
        DateTime newExpiresAt,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default
    )
    {
        var newToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = newTokenHash,
            ExpiresAt = newExpiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };
        _context.RefreshTokens.Add(newToken);

        // Flush to get the new token's Id
        await _context.SaveChangesAsync(cancellationToken);

        // Revoke old token and chain to new
        await _context
            .RefreshTokens.Where(rt => rt.Id == oldTokenId)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
                        .SetProperty(rt => rt.ReplacedByTokenId, newToken.Id),
                cancellationToken
            );
    }

    public async Task RevokeByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default
    )
    {
        // Idempotent — no-op if already revoked or not found
        await _context
            .RefreshTokens.Where(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow),
                cancellationToken
            );
    }

    public async Task RevokeAllForUserAsync(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        await _context
            .RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow),
                cancellationToken
            );
    }
}

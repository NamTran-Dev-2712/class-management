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
}

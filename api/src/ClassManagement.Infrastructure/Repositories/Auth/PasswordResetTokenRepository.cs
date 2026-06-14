using ClassManagement.Infrastructure.Persistence.DbContext;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Set-based invalidation of any active token — uses partial index idx_prt_user_active.
    public Task InvalidatePreviousAsync(
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .PasswordResetTokens.Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow),
                cancellationToken
            );

    public async Task CreateAsync(
        long userId,
        string tokenHash,
        DateTime expiresAt,
        CancellationToken cancellationToken = default
    )
    {
        _context.PasswordResetTokens.Add(
            new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
            }
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<PasswordResetToken?> FindActiveByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .PasswordResetTokens.Where(t => t.UserId == userId && t.UsedAt == null)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task IncrementAttemptAsync(
        long tokenId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .PasswordResetTokens.Where(t => t.Id == tokenId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.AttemptCount, t => t.AttemptCount + 1),
                cancellationToken
            );

    public Task MarkUsedAsync(long tokenId, CancellationToken cancellationToken = default) =>
        _context
            .PasswordResetTokens.Where(t => t.Id == tokenId && t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow),
                cancellationToken
            );
}

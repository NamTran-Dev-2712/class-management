namespace ClassManagement.Domain.Modules.Auth.Entities;

// Append-only — no UpdatedAt, no audit columns
public sealed class RefreshToken : BaseEntity
{
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public long? ReplacedByTokenId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

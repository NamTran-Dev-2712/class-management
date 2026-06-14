namespace ClassManagement.Domain.Modules.Auth.Entities;

// Append-only — no UpdatedAt
public sealed class PasswordResetToken : BaseEntity
{
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    // Number of failed verification attempts — caps brute-force on the 6-digit OTP
    public int AttemptCount { get; set; }
}

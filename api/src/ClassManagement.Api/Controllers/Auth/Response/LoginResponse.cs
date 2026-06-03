public class LoginResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool EmailConfirmed { get; set; }

    public void FromAuthResult(AuthResult authResult)
    {
        PublicId = authResult.PublicId;
        DisplayName = authResult.DisplayName;
        Email = authResult.Email;
        ExpiresAt = authResult.ExpiresAt;
        RefreshTokenExpiresAt = authResult.RefreshTokenExpiresAt;
        Roles = authResult.Roles;
        EmailConfirmed = authResult.EmailConfirmed;
    }
}

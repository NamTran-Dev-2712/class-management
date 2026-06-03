public record UserTokenData
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
}

public interface IJwtTokenService
{
    Task<TokenResult> GenerateTokensAsync(UserTokenData data);
}

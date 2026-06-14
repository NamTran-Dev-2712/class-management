using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClassManagement.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly ITokenHasher _tokenHasher;

    public JwtTokenService(IOptions<JwtOptions> options, ITokenHasher tokenHasher)
    {
        _options = options.Value;
        _tokenHasher = tokenHasher;
    }

    public Task<TokenResult> GenerateTokensAsync(UserTokenData data)
    {
        var now = DateTime.UtcNow;
        var accessToken = BuildAccessToken(data, now);
        var rawRefreshToken = GenerateRawRefreshToken();

        return Task.FromResult(
            new TokenResult(
                AccessToken: accessToken,
                RefreshToken: rawRefreshToken,
                RefreshTokenHash: _tokenHasher.Hash(rawRefreshToken),
                AccessTokenExpiry: now.AddMinutes(_options.ExpiryMinutes),
                RefreshTokenExpiry: now.AddDays(_options.RefreshTokenExpiryDays)
            )
        );
    }

    private string BuildAccessToken(UserTokenData data, DateTime now)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, data.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, data.Email),
            new(JwtRegisteredClaimNames.Name, data.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in data.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRawRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}

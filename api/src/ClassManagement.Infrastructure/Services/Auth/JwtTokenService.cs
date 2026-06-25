using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly ITokenHasher _tokenHasher;
    private readonly ISystemSettingsService _settings;

    public JwtTokenService(
        IOptions<JwtOptions> options,
        ITokenHasher tokenHasher,
        ISystemSettingsService settings
    )
    {
        _options = options.Value;
        _tokenHasher = tokenHasher;
        _settings = settings;
    }

    public async Task<TokenResult> GenerateTokensAsync(UserTokenData data)
    {
        var now = DateTime.UtcNow;
        var accessToken = BuildAccessToken(data, now);
        var rawRefreshToken = GenerateRawRefreshToken();

        // Refresh-token lifetime is live-configurable (system_settings) with the appsettings fallback.
        var refreshTtlDays = await _settings.GetIntAsync(
            SystemSettingKeys.RefreshTokenTtlDays,
            _options.RefreshTokenExpiryDays
        );

        return new TokenResult(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            RefreshTokenHash: _tokenHasher.Hash(rawRefreshToken),
            AccessTokenExpiry: now.AddMinutes(_options.ExpiryMinutes),
            RefreshTokenExpiry: now.AddDays(refreshTtlDays)
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

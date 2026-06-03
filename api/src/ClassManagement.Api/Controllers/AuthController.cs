using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly ISender _mediator;
    private readonly IWebHostEnvironment _env;

    public AuthController(ISender mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var userId = await _mediator.Send(command);
        return ApiCreated(
            routeName: null,
            routeValues: null,
            data: new { UserId = userId },
            message: "User registered successfully."
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);

        SetAuthCookies(
            result.AccessToken,
            result.ExpiresAt,
            result.RefreshToken,
            result.RefreshTokenExpiresAt
        );

        return ApiOk(ToProfileDto(result), "Login successful.");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetProfileQuery(), cancellationToken);
        return ApiOk(profile);
    }

    private void SetAuthCookies(
        string accessToken,
        DateTime accessExpiry,
        string refreshToken,
        DateTime refreshExpiry
    )
    {
        var isSecure = !_env.IsDevelopment();

        Response.Cookies.Append(
            "access_token",
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = accessExpiry,
            }
        );

        Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = refreshExpiry,
            }
        );
    }

    private static UserProfileDto ToProfileDto(AuthResult r) =>
        new(
            PublicId: r.PublicId,
            DisplayName: r.DisplayName,
            Email: r.Email,
            EmailConfirmed: r.EmailConfirmed,
            AvatarUrl: r.AvatarUrl,
            Bio: r.Bio,
            LastLoginAt: r.LastLoginAt,
            CreatedAt: r.CreatedAt,
            Roles: r.Roles
        );
}

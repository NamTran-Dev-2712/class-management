using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting(RateLimitOptions.Policies.Register)]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var userId = await _mediator.Send(command);
        return ApiCreated(
            routeName: null,
            routeValues: null,
            data: new { UserId = userId },
            message: "Auth.RegisterSuccess"
        );
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitOptions.Policies.Login)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        SetAuthCookies(
            result.AccessToken,
            result.ExpiresAt,
            result.RefreshToken,
            result.RefreshTokenExpiresAt
        );
        return ApiOk(ToProfileDto(result), "Auth.LoginSuccess");
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitOptions.Policies.Refresh)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(rawToken))
            return ApiUnauthorized("Auth.RefreshTokenMissing");

        var result = await _mediator.Send(
            new RefreshTokenCommand(
                rawToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ),
            cancellationToken
        );

        SetAuthCookies(
            result.AccessToken,
            result.ExpiresAt,
            result.RefreshToken,
            result.RefreshTokenExpiresAt
        );
        return ApiOk(ToProfileDto(result));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetProfileQuery(), cancellationToken);
        return ApiOk(profile);
    }

    [Authorize]
    [HttpPatch("profile")]
    [EnableRateLimiting(RateLimitOptions.Policies.UpdateProfile)]
    public async Task<IActionResult> UpdateProfile(
        UpdateProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        var profile = await _mediator.Send(command, cancellationToken);
        return ApiOk(profile);
    }

    [Authorize]
    [HttpPost("change-password")]
    [EnableRateLimiting(RateLimitOptions.Policies.ChangePassword)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command, cancellationToken);
        DeleteAuthCookies();
        return ApiOk("Auth.PasswordChanged");
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimitOptions.Policies.ForgotPassword)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command, cancellationToken);
        // Always generic — never reveal whether the email exists.
        return ApiOk("Auth.ForgotPasswordSent");
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting(RateLimitOptions.Policies.ResetPassword)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command, cancellationToken);
        DeleteAuthCookies();
        return ApiOk("Auth.PasswordResetSuccess");
    }

    [HttpPost("logout")]
    [EnableRateLimiting(RateLimitOptions.Policies.Logout)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies["refresh_token"];
        await _mediator.Send(new LogoutCommand(rawToken), cancellationToken);
        DeleteAuthCookies();
        return ApiOk("Auth.LogoutSuccess");
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

    private void DeleteAuthCookies()
    {
        var opts = new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax };
        Response.Cookies.Delete("access_token", opts);
        Response.Cookies.Delete("refresh_token", opts);
    }

    private static UserProfileDto ToProfileDto(AuthResult r) =>
        new(
            PublicId: r.PublicId,
            DisplayName: r.DisplayName,
            PhoneNumber: r.PhoneNumber,
            Email: r.Email,
            EmailConfirmed: r.EmailConfirmed,
            AvatarUrl: r.AvatarUrl,
            Bio: r.Bio,
            LastLoginAt: r.LastLoginAt,
            CreatedAt: r.CreatedAt,
            Roles: r.Roles
        );
}

using ClassManagement.Application.Interfaces.Localization;

namespace ClassManagement.Api.Contracts.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    // Resolve message keys (e.g. "Auth.LoginSuccess") to the current request culture.
    private string L(string key) =>
        HttpContext.RequestServices.GetRequiredService<ILocalizationService>().Translate(key);

    protected IActionResult ApiOk<T>(T data, string message = "Response.Success") =>
        base.Ok(ApiResponse<T>.Ok(data, HttpContext.TraceIdentifier, L(message)));

    protected IActionResult ApiOk(string message = "Response.Success") =>
        base.Ok(ApiResponse<object?>.Ok(null, HttpContext.TraceIdentifier, L(message)));

    protected IActionResult ApiCreated<T>(
        string? routeName,
        object? routeValues,
        T data,
        string message = "Response.Created"
    )
    {
        var response = ApiResponse<T>.Ok(
            data,
            HttpContext.TraceIdentifier,
            L(message),
            StatusCodes.Status201Created
        );
        return routeName is not null
            ? CreatedAtRoute(routeName, routeValues, response)
            : StatusCode(StatusCodes.Status201Created, response);
    }

    protected IActionResult ApiNoContent() =>
        base.Ok(
            ApiResponse<object?>.Ok(null, HttpContext.TraceIdentifier, L("Response.NoContent"))
        );

    protected IActionResult ApiBadRequest(string message, IEnumerable<string>? errors = null) =>
        base.BadRequest(
            ApiResponse<object?>.Fail(
                StatusCodes.Status400BadRequest,
                L(message),
                errors,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiNotFound(string message = "Resource.NotFound") =>
        base.NotFound(
            ApiResponse<object?>.Fail(
                StatusCodes.Status404NotFound,
                L(message),
                null,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiForbidden(string message = "Access.Forbidden") =>
        StatusCode(
            StatusCodes.Status403Forbidden,
            ApiResponse<object?>.Fail(
                StatusCodes.Status403Forbidden,
                L(message),
                null,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiUnauthorized(string message = "Authentication.Required") =>
        base.Unauthorized(
            ApiResponse<object?>.Fail(
                StatusCodes.Status401Unauthorized,
                L(message),
                null,
                HttpContext.TraceIdentifier
            )
        );
}

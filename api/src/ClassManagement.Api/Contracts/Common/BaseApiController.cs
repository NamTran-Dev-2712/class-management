namespace ClassManagement.Api.Contracts.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiOk<T>(T data, string message = "Response.Success") =>
        base.Ok(ApiResponse<T>.Ok(data, HttpContext.TraceIdentifier, message));

    protected IActionResult ApiOk(string message = "Response.Success") =>
        base.Ok(ApiResponse<object?>.Ok(null, HttpContext.TraceIdentifier, message));

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
            message,
            StatusCodes.Status201Created
        );
        return routeName is not null
            ? CreatedAtRoute(routeName, routeValues, response)
            : StatusCode(StatusCodes.Status201Created, response);
    }

    protected IActionResult ApiNoContent() =>
        base.Ok(ApiResponse<object?>.Ok(null, HttpContext.TraceIdentifier, "Response.NoContent"));

    protected IActionResult ApiBadRequest(string message, IEnumerable<string>? errors = null) =>
        base.BadRequest(
            ApiResponse<object?>.Fail(
                StatusCodes.Status400BadRequest,
                message,
                errors,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiNotFound(string message = "Resource.NotFound") =>
        base.NotFound(
            ApiResponse<object?>.Fail(
                StatusCodes.Status404NotFound,
                message,
                null,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiForbidden(string message = "Access.Forbidden") =>
        StatusCode(
            StatusCodes.Status403Forbidden,
            ApiResponse<object?>.Fail(
                StatusCodes.Status403Forbidden,
                message,
                null,
                HttpContext.TraceIdentifier
            )
        );

    protected IActionResult ApiUnauthorized(string message = "Authentication.Required") =>
        base.Unauthorized(
            ApiResponse<object?>.Fail(
                StatusCodes.Status401Unauthorized,
                message,
                null,
                HttpContext.TraceIdentifier
            )
        );
}

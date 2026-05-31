using Microsoft.AspNetCore.Diagnostics;

namespace ClassManagement.Api.Contracts.Exceptions;

// Registered via services.AddExceptionHandler<GlobalExceptionHandler>()
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct
    )
    {
        var (statusCode, message, errors) = exception switch
        {
            // ValidationException must precede BadException (it's a subclass)
            ValidationException vex => (
                StatusCodes.Status400BadRequest,
                vex.Message,
                (IEnumerable<string>?)vex.ValidationErrors
            ),
            BadException bex => (StatusCodes.Status400BadRequest, bex.Message, null),
            NotFoundException nfex => (StatusCodes.Status404NotFound, nfex.Message, null),
            ConflictException cex => (StatusCodes.Status409Conflict, cex.Message, null),
            ForbiddenException fex => (StatusCodes.Status403Forbidden, fex.Message, null),
            UnauthorizedException uex => (StatusCodes.Status401Unauthorized, uex.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null),
        };

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception [{StatusCode}]: {Message}", statusCode, message);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(
            statusCode,
            message,
            errors,
            httpContext.TraceIdentifier
        );
        await httpContext.Response.WriteAsJsonAsync(response, ct);

        return true;
    }
}

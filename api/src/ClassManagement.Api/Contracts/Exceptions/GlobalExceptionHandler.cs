using ClassManagement.Application.Interfaces.Localization;
using Microsoft.AspNetCore.Diagnostics;

namespace ClassManagement.Api.Contracts.Exceptions;

// Registered via services.AddExceptionHandler<GlobalExceptionHandler>()
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ILocalizationService _localizer;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ILocalizationService localizer
    )
    {
        _logger = logger;
        _localizer = localizer;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct
    )
    {
        var (statusCode, messageKey, errorKeys) = exception switch
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
            _ => (StatusCodes.Status500InternalServerError, "Error.Unexpected", null),
        };

        // Exception messages and validation errors carry message keys — localize them
        // for the request culture. Unknown keys pass through unchanged.
        var message = _localizer[messageKey];
        var errors = errorKeys?.Select(key => _localizer[key]);

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning(
                "Handled exception [{StatusCode}]: {MessageKey}",
                statusCode,
                messageKey
            );

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

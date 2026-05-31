namespace ClassManagement.Api.Contracts.Common;

/// <summary>
/// Unified API response envelope used by all endpoints.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public T? Data { get; private init; }
    public IReadOnlyList<string>? Errors { get; private init; }
    public int StatusCode { get; private init; }
    public string TraceId { get; private init; } = string.Empty;
    public DateTimeOffset Timestamp { get; private init; }

    private ApiResponse() { }

    public static ApiResponse<T> Ok(
        T data,
        string traceId,
        string message = "Response.Success",
        int statusCode = StatusCodes.Status200OK
    ) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null,
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTimeOffset.UtcNow,
        };

    public static ApiResponse<T> Fail(
        int statusCode,
        string message,
        IEnumerable<string>? errors,
        string traceId
    ) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors?.ToList().AsReadOnly(),
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTimeOffset.UtcNow,
        };
}

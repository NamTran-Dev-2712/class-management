public record UpdateUserCommand(string DisplayName, string Role, string? PhoneNumber) : IRequest
{
    // Bound from the route, not the request body (email is intentionally immutable on update).
    public Guid PublicId { get; init; }
}

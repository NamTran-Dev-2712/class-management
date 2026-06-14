public record CreateUserCommand(string DisplayName, string Email, string Role, string? PhoneNumber)
    : IRequest<Guid>;

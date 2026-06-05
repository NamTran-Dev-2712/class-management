public record LogoutCommand(string? RawRefreshToken) : IRequest;

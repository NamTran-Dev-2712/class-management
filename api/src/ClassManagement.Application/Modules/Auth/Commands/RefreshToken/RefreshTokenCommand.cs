public record RefreshTokenCommand(string RawToken, string? IpAddress, string? UserAgent)
    : IRequest<AuthResult>;

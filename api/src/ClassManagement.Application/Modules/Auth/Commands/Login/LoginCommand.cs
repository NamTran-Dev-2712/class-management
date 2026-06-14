using MediatR;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

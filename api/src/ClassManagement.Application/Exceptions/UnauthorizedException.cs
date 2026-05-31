namespace ClassManagement.Application.Exceptions;

public sealed class UnauthorizedException : AppDomainException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base(message) { }
}

namespace ClassManagement.Application.Exceptions;

public sealed class ForbiddenException : AppDomainException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

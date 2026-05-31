namespace ClassManagement.Application.Exceptions;

public sealed class ConflictException : AppDomainException
{
    public ConflictException(string message)
        : base(message) { }
}

namespace ClassManagement.Application.Exceptions;

public class BadException : AppDomainException
{
    public BadException(string message)
        : base(message) { }
}

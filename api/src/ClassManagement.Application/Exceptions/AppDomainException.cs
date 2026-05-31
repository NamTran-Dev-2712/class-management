namespace ClassManagement.Application.Exceptions;

public abstract class AppDomainException : Exception
{
    protected AppDomainException(string message)
        : base(message) { }
}

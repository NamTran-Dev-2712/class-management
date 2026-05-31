using FluentValidation.Results;

namespace ClassManagement.Application.Exceptions;

// Must be listed before BadException in GlobalExceptionHandler pattern match
public sealed class ValidationException : BadException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Validation.Failed")
    {
        ValidationErrors = failures.Select(f => f.ErrorMessage).ToList().AsReadOnly();
    }
}

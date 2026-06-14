using ClassManagement.Application.Common.Constants;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Validation.DisplayName.Required")
            .MinimumLength(2)
            .WithMessage("Validation.DisplayName.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.DisplayName.MaxLength");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.Email.Required")
            .MaximumLength(256)
            .WithMessage("Validation.Email.MaxLength")
            .EmailAddress()
            .WithMessage("Validation.Email.Invalid");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Validation.Role.Required")
            .Must(role => ApplicationRoles.Manageable.Contains(role))
            .WithMessage("Validation.Role.Invalid");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-()]{6,20}$")
            .WithMessage("Validation.PhoneNumber.Invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

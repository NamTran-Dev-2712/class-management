using ClassManagement.Application.Common.Constants;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Validation.DisplayName.Required")
            .MinimumLength(2)
            .WithMessage("Validation.DisplayName.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.DisplayName.MaxLength");

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

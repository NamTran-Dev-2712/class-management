using FluentValidation;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.Email.Required")
            .EmailAddress()
            .WithMessage("Validation.Email.Invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Validation.Password.Required")
            .MinimumLength(8)
            .WithMessage("Validation.Password.MinLength");
    }
}

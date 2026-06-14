public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.Email.Required")
            .MaximumLength(256)
            .WithMessage("Validation.Email.MaxLength")
            .EmailAddress()
            .WithMessage("Validation.Email.Invalid");
    }
}

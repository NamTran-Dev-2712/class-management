public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.Email.Required")
            .EmailAddress()
            .WithMessage("Validation.Email.Invalid");

        RuleFor(x => x.Otp)
            .NotEmpty()
            .WithMessage("Validation.Otp.Required")
            .Matches("^[0-9]{6}$")
            .WithMessage("Validation.Otp.Format");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Validation.Password.NewRequired")
            .MinimumLength(8)
            .WithMessage("Validation.Password.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.Password.MaxLength")
            .Matches("[A-Z]")
            .WithMessage("Validation.Password.Uppercase")
            .Matches("[a-z]")
            .WithMessage("Validation.Password.Lowercase")
            .Matches("[0-9]")
            .WithMessage("Validation.Password.Number")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Validation.Password.Special");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Validation.Password.ConfirmRequired")
            .Equal(x => x.NewPassword)
            .WithMessage("Validation.Password.Mismatch");
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Validation.Password.CurrentRequired");

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
            .WithMessage("Validation.Password.Special")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Validation.Password.MustDiffer");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Validation.Password.ConfirmRequired")
            .Equal(x => x.NewPassword)
            .WithMessage("Validation.Password.Mismatch");
    }
}

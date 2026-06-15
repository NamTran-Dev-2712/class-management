public class JoinClassValidator : AbstractValidator<JoinClassCommand>
{
    public JoinClassValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty()
            .WithMessage("Validation.Class.InviteCodeRequired")
            .Matches("^[A-Za-z0-9]{6,8}$")
            .WithMessage("Validation.Class.InviteCodeFormat");
    }
}

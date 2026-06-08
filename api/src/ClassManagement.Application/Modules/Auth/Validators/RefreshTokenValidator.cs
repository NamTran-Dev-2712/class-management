public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RawToken).NotEmpty().WithMessage("Validation.RefreshToken.Required");
    }
}

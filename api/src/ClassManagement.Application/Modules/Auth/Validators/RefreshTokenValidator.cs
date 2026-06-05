public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RawToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

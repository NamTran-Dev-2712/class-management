public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Validation.DisplayName.Required")
            .MinimumLength(2)
            .WithMessage("Validation.DisplayName.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.DisplayName.MaxLength")
            .Must(name => !name.Any(c => c is '<' or '>' or '&'))
            .WithMessage("Validation.DisplayName.Invalid");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Validation.Bio.MaxLength")
            .When(x => x.Bio is not null);

        RuleFor(x => x.AvatarUrl)
            .Must(url => url!.StartsWith("http://") || url.StartsWith("https://"))
            .WithMessage("Validation.AvatarUrl.Invalid")
            .When(x => x.AvatarUrl is not null);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$")
            .WithMessage("Validation.PhoneNumber.Invalid")
            .When(x => x.PhoneNumber is not null);
    }
}

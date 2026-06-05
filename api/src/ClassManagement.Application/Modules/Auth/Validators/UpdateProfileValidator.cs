public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MinimumLength(2)
            .WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Display name must not exceed 100 characters.")
            .Must(name => !name.Any(c => c is '<' or '>' or '&'))
            .WithMessage("Display name contains invalid characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must not exceed 500 characters.")
            .When(x => x.Bio is not null);

        RuleFor(x => x.AvatarUrl)
            .Must(url => url!.StartsWith("http://") || url.StartsWith("https://"))
            .WithMessage("Avatar URL must start with http:// or https://.")
            .When(x => x.AvatarUrl is not null);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$")
            .WithMessage("Phone number format is invalid.")
            .When(x => x.PhoneNumber is not null);
    }
}

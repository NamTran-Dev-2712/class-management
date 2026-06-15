public class UpdateClassValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.Class.NameRequired")
            .MinimumLength(2)
            .WithMessage("Validation.Class.NameLength")
            .MaximumLength(200)
            .WithMessage("Validation.Class.NameLength");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Validation.Class.DescriptionMaxLength")
            .When(x => x.Description is not null);

        RuleFor(x => x.SubjectName)
            .MaximumLength(200)
            .WithMessage("Validation.Class.SubjectNameMaxLength")
            .When(x => x.SubjectName is not null);

        RuleFor(x => x.CoverImageUrl)
            .Matches("^https?://")
            .WithMessage("Validation.Class.CoverImageUrlInvalid")
            .MaximumLength(2048)
            .WithMessage("Validation.Class.CoverImageUrlInvalid")
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl));
    }
}

public class CreateSubjectValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.Subject.NameRequired")
            .MinimumLength(2)
            .WithMessage("Validation.Subject.NameLength")
            .MaximumLength(100)
            .WithMessage("Validation.Subject.NameLength");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Validation.Subject.DescriptionMaxLength")
            .When(x => x.Description is not null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Validation.Subject.DisplayOrderInvalid");
    }
}

// Shared field rules for the create/update exam metadata commands (BR-4-01 relaxed: subject optional).
// Message keys are dotted i18n keys resolved centrally; never inline English.
public abstract class ExamWriteValidator<T> : AbstractValidator<T>
    where T : IExamWriteCommand
{
    protected ExamWriteValidator()
    {
        RuleFor(x => x.Visibility).IsInEnum().WithMessage("Validation.Exam.VisibilityInvalid");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Validation.Exam.TitleRequired")
            .MinimumLength(3)
            .WithMessage("Validation.Exam.TitleLength")
            .MaximumLength(300)
            .WithMessage("Validation.Exam.TitleLength");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Validation.Exam.DescriptionMaxLength")
            .When(x => x.Description is not null);
    }
}

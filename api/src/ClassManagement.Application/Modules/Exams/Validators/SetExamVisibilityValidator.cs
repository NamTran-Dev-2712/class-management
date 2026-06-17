public sealed class SetExamVisibilityValidator : AbstractValidator<SetExamVisibilityCommand>
{
    public SetExamVisibilityValidator()
    {
        RuleFor(x => x.Visibility).IsInEnum().WithMessage("Validation.Exam.VisibilityInvalid");
    }
}

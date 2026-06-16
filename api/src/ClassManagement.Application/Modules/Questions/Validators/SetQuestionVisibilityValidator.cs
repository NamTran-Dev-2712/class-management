public sealed class SetQuestionVisibilityValidator : AbstractValidator<SetQuestionVisibilityCommand>
{
    public SetQuestionVisibilityValidator()
    {
        RuleFor(x => x.Visibility).IsInEnum().WithMessage("Validation.Question.VisibilityInvalid");
    }
}

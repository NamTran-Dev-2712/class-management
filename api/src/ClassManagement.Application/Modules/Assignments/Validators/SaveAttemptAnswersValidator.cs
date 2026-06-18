// Validates an auto-save batch: the list must be present and each writing answer within length.
public sealed class SaveAttemptAnswersValidator : AbstractValidator<SaveAttemptAnswersCommand>
{
    private const int MaxTextAnswerLength = 50000;

    public SaveAttemptAnswersValidator()
    {
        RuleFor(x => x.Answers).NotNull().WithMessage("Validation.Attempt.AnswersNull");

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer
                    .RuleFor(a => a.TextAnswer)
                    .MaximumLength(MaxTextAnswerLength)
                    .WithMessage("Validation.Attempt.TextTooLong")
                    .When(a => a.TextAnswer is not null);
            });
    }
}

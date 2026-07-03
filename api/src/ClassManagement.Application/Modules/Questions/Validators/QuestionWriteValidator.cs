using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Shared field + per-type option rules for the create/update question commands (BR-3-02..05, 3-10).
// Message keys are dotted i18n keys resolved centrally; never inline English.
public abstract class QuestionWriteValidator<T> : AbstractValidator<T>
    where T : IQuestionWriteCommand
{
    private const int MinChoiceOptions = 2;
    private const int MaxOptionLength = 2000;
    private const int MaxAttachments = 10;

    protected QuestionWriteValidator()
    {
        // Subject is optional (a teacher can create a question before any catalog subject exists).
        // When supplied, the handler verifies it is an active subject.
        RuleFor(x => x.Type).IsInEnum().WithMessage("Validation.Question.TypeInvalid");
        RuleFor(x => x.Difficulty).IsInEnum().WithMessage("Validation.Question.DifficultyInvalid");
        RuleFor(x => x.Visibility).IsInEnum().WithMessage("Validation.Question.VisibilityInvalid");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Validation.Question.ContentRequired")
            .MinimumLength(10)
            .WithMessage("Validation.Question.ContentLength")
            .MaximumLength(10000)
            .WithMessage("Validation.Question.ContentLength");

        RuleFor(x => x.SuggestedPoint)
            .GreaterThan(0)
            .WithMessage("Validation.Question.SuggestedPointRange")
            .LessThanOrEqualTo(100)
            .WithMessage("Validation.Question.SuggestedPointRange");

        RuleFor(x => x.Explanation)
            .MaximumLength(2000)
            .WithMessage("Validation.Question.ExplanationMaxLength")
            .When(x => x.Explanation is not null);

        RuleFor(x => x.Tags!)
            .Must(tags => tags.Count <= QuestionTagNormalizer.MaxTagsPerQuestion)
            .WithMessage("Validation.Question.TooManyTags")
            .When(x => x.Tags is not null);

        // Question-level media attachments (MVP-9) — bounded count.
        RuleFor(x => x.Attachments!)
            .Must(a => a.Count <= MaxAttachments)
            .WithMessage("Validation.Question.TooManyAttachments")
            .When(x => x.Attachments is not null);

        // Per-type option rules, pushed with contextual message keys.
        RuleFor(x => x).Custom(ValidateOptions);
    }

    private static void ValidateOptions(T command, ValidationContext<T> context)
    {
        var options = command.Options ?? [];

        switch (command.Type)
        {
            case QuestionType.ShortWriting or QuestionType.LongWriting:
                return; // No options for writing types — ignored even if supplied.

            case QuestionType.SingleChoice:
                RequireOptionContents(options, context);
                if (options.Count < MinChoiceOptions)
                    context.AddFailure("Options", "Validation.Question.OptionsMin");
                if (options.Count(o => o.IsCorrect) != 1)
                    context.AddFailure("Options", "Validation.Question.SingleChoiceOneCorrect");
                break;

            case QuestionType.MultipleChoice:
                RequireOptionContents(options, context);
                if (options.Count < MinChoiceOptions)
                    context.AddFailure("Options", "Validation.Question.OptionsMin");
                if (!options.Any(o => o.IsCorrect))
                    context.AddFailure("Options", "Validation.Question.MultipleChoiceOneCorrect");
                break;

            case QuestionType.TrueFalse:
                if (options.Count != 2)
                    context.AddFailure("Options", "Validation.Question.TrueFalseTwoOptions");
                if (options.Count(o => o.IsCorrect) != 1)
                    context.AddFailure("Options", "Validation.Question.TrueFalseOneCorrect");
                break;
        }
    }

    private static void RequireOptionContents(
        IReadOnlyList<QuestionOptionInput> options,
        ValidationContext<T> context
    )
    {
        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Content))
                context.AddFailure("Options", "Validation.Question.OptionContentRequired");
            else if (option.Content.Length > MaxOptionLength)
                context.AddFailure("Options", "Validation.Question.OptionContentLength");
        }
    }
}

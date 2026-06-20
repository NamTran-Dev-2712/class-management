// Validates a manual-grading batch: at least one item, distinct questions, non-negative score, feedback
// within length. The per-question max-point bound is enforced in the handler (needs the snapshot point).
public sealed class GradeAttemptValidator : AbstractValidator<GradeAttemptCommand>
{
    private const int MaxFeedbackLength = 5000;

    public GradeAttemptValidator()
    {
        RuleFor(x => x.Grades).NotEmpty().WithMessage("Validation.Grade.GradesRequired");

        RuleFor(x => x.Grades)
            .Must(g => g.Select(i => i.QuestionPublicId).Distinct().Count() == g.Count)
            .WithMessage("Validation.Grade.DuplicateQuestion")
            .When(x => x.Grades is not null && x.Grades.Count > 0);

        RuleForEach(x => x.Grades)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Score)
                    .GreaterThanOrEqualTo(0m)
                    .WithMessage("Validation.Grade.ScoreNegative");

                item.RuleFor(i => i.Feedback)
                    .MaximumLength(MaxFeedbackLength)
                    .WithMessage("Validation.Grade.FeedbackTooLong")
                    .When(i => i.Feedback is not null);
            });
    }
}

// Shared field rules for the create/update assignment commands. Caps come from IAssignmentPolicy
// (config-driven, no hardcoded tunables). Message keys are dotted i18n keys resolved centrally.
public abstract class AssignmentWriteValidator<T> : AbstractValidator<T>
    where T : IAssignmentWriteCommand
{
    protected AssignmentWriteValidator(IAssignmentPolicy policy)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Validation.Assignment.TitleRequired")
            .MinimumLength(3)
            .WithMessage("Validation.Assignment.TitleLength")
            .MaximumLength(300)
            .WithMessage("Validation.Assignment.TitleLength");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Validation.Assignment.DescriptionMaxLength")
            .When(x => x.Description is not null);

        RuleFor(x => x.ScorePolicy)
            .IsInEnum()
            .WithMessage("Validation.Assignment.ScorePolicyInvalid");

        RuleFor(x => x.GradePublishPolicy)
            .IsInEnum()
            .WithMessage("Validation.Assignment.GradePublishPolicyInvalid");

        RuleFor(x => x.MaxAttempts)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Validation.Assignment.MaxAttemptsInvalid")
            .LessThanOrEqualTo(policy.MaxAttemptsCap)
            .WithMessage("Validation.Assignment.MaxAttemptsInvalid");

        RuleFor(x => x.TimeLimitMinutes!.Value)
            .GreaterThan(0)
            .WithMessage("Validation.Assignment.TimeLimitInvalid")
            .LessThanOrEqualTo(policy.MaxTimeLimitMinutes)
            .WithMessage("Validation.Assignment.TimeLimitInvalid")
            .When(x => x.TimeLimitMinutes.HasValue);

        // BR-5-03: closes_at must be after opens_at (when both set).
        RuleFor(x => x.ClosesAt)
            .Must((cmd, closes) => closes > cmd.OpensAt)
            .WithMessage("Validation.Assignment.TimeWindowInvalid")
            .When(x => x.OpensAt.HasValue && x.ClosesAt.HasValue);

        // BR-5-04: time limit must fit inside the open→close window (when all three set).
        RuleFor(x => x)
            .Must(cmd =>
                cmd.TimeLimitMinutes!.Value
                <= (cmd.ClosesAt!.Value - cmd.OpensAt!.Value).TotalMinutes
            )
            .WithMessage("Validation.Assignment.TimeLimitExceedsWindow")
            .When(x => x.TimeLimitMinutes.HasValue && x.OpensAt.HasValue && x.ClosesAt.HasValue);
    }
}

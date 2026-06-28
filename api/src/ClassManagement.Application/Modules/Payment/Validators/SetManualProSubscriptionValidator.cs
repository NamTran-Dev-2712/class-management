// Shape checks for the admin manual-set-Pro command (MVP-8). Business checks live in the handler.
public class SetManualProSubscriptionValidator : AbstractValidator<SetManualProSubscriptionCommand>
{
    public SetManualProSubscriptionValidator()
    {
        RuleFor(x => x.TeacherPublicId)
            .NotEmpty()
            .WithMessage("Validation.Subscription.TeacherRequired");

        RuleFor(x => x.PlanPublicId).NotEmpty().WithMessage("Validation.Subscription.PlanRequired");

        RuleFor(x => x.AdminNote)
            .MaximumLength(500)
            .WithMessage("Validation.Subscription.NoteTooLong")
            .When(x => !string.IsNullOrEmpty(x.AdminNote));
    }
}

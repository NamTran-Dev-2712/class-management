using ClassManagement.Domain.Modules.Admin.Enums;

public sealed class ReviewReportValidator : AbstractValidator<ReviewReportCommand>
{
    public ReviewReportValidator()
    {
        // Pending is the initial state, not a review outcome.
        RuleFor(x => x.Status)
            .Must(s =>
                s is ReportStatus.Reviewing or ReportStatus.Resolved or ReportStatus.Rejected
            )
            .WithMessage("Validation.Report.StatusInvalid");

        // Resolving requires an explicit action to apply.
        RuleFor(x => x.AdminAction)
            .NotNull()
            .WithMessage("Validation.Report.ActionRequired")
            .When(x => x.Status == ReportStatus.Resolved);

        RuleFor(x => x.AdminNote)
            .MaximumLength(1000)
            .WithMessage("Validation.Report.NoteTooLong")
            .When(x => !string.IsNullOrEmpty(x.AdminNote));
    }
}

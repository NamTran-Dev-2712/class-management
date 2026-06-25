public sealed class SubmitReportValidator : AbstractValidator<SubmitReportCommand>
{
    public SubmitReportValidator()
    {
        RuleFor(x => x.TargetType).IsInEnum().WithMessage("Validation.Report.TargetTypeInvalid");
        RuleFor(x => x.Reason).IsInEnum().WithMessage("Validation.Report.ReasonInvalid");
        RuleFor(x => x.TargetPublicId).NotEmpty().WithMessage("Validation.Report.TargetRequired");
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Validation.Report.DescriptionTooLong")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

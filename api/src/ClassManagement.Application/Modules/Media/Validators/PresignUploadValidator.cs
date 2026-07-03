// Shape validation for a presign request (MVP-9). Business rules (allowlist, per-file cap, quota) live in
// the handler where the live policy + owner usage are available.
public sealed class PresignUploadValidator : AbstractValidator<PresignUploadCommand>
{
    public PresignUploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("Validation.Media.FileName.Required")
            .MaximumLength(255)
            .WithMessage("Validation.Media.FileName.MaxLength");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Validation.Media.ContentType.Required")
            .MaximumLength(100)
            .WithMessage("Validation.Media.ContentType.MaxLength");

        RuleFor(x => x.ByteSize).GreaterThan(0).WithMessage("Validation.Media.ByteSize.Positive");

        RuleFor(x => x.Width!.Value)
            .GreaterThan(0)
            .When(x => x.Width.HasValue)
            .WithMessage("Validation.Media.Dimension.Positive");
        RuleFor(x => x.Height!.Value)
            .GreaterThan(0)
            .When(x => x.Height.HasValue)
            .WithMessage("Validation.Media.Dimension.Positive");
        RuleFor(x => x.DurationSeconds!.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DurationSeconds.HasValue)
            .WithMessage("Validation.Media.Dimension.Positive");
    }
}

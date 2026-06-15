public class RejectMemberValidator : AbstractValidator<RejectMemberCommand>
{
    public RejectMemberValidator()
    {
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .WithMessage("Validation.Class.RejectionReasonMaxLength")
            .When(x => x.RejectionReason is not null);
    }
}

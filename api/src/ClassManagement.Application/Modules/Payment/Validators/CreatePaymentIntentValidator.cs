public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentValidator()
    {
        RuleFor(x => x.PlanPublicId).NotEmpty().WithMessage("Validation.Payment.PlanRequired");
        RuleFor(x => x.Provider).IsInEnum().WithMessage("Validation.Payment.ProviderInvalid");
    }
}

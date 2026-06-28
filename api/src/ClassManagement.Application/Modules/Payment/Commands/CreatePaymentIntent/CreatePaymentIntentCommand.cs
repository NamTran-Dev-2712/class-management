using ClassManagement.Application.Modules.Payment.DTOs;

// Start a checkout for a paid plan (MVP-8): creates a Pending payment order and asks the chosen provider
// for a redirect/QR. The subscription itself is only activated later, by the verified webhook.
public record CreatePaymentIntentCommand(Guid PlanPublicId, PaymentProvider Provider)
    : IRequest<CheckoutResultDto>;

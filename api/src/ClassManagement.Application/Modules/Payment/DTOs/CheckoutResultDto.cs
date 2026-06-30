namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>The result of starting a checkout (MVP-8): the created payment + where to send the payer.</summary>
public sealed record CheckoutResultDto(
    Guid PaymentPublicId,
    string Provider,
    long AmountVnd,
    string? RedirectUrl,
    string? QrCodeUrl
);

/// <summary>Current status of a payment order (for the checkout page's polling fallback).</summary>
public sealed record PaymentStatusDto(
    Guid PublicId,
    string Provider,
    string Status,
    long AmountVnd,
    string BillingCycle,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

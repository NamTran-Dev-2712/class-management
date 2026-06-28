namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>One row in a teacher's invoice history (MVP-8). Immutable record of a completed payment.</summary>
public sealed record InvoiceDto(
    Guid PublicId,
    string InvoiceNumber,
    long AmountVnd,
    string PlanName,
    string BillingCycle,
    DateTime IssuedAt
);

/// <summary>
/// The data needed to render an invoice document (MVP-8). Resolved by the query (guarded to the owner /
/// admin); the controller passes localized labels to the PDF service so the renderer stays i18n-free.
/// </summary>
public sealed record InvoiceDocumentDto(
    string InvoiceNumber,
    DateTime IssuedAt,
    string PlanName,
    string BillingCycle,
    long AmountVnd,
    string BilledToEmail
);

using ClassManagement.Application.Modules.Payment.DTOs;

namespace ClassManagement.Application.Modules.Payment.Interfaces;

/// <summary>
/// Renders an immutable invoice to a PDF byte stream (MVP-8). Localization-free by design — the caller
/// (controller) resolves the labels for the request culture and passes them in, mirroring the CSV grade
/// export service. Implemented in Infrastructure (QuestPDF).
/// </summary>
public interface IInvoicePdfService
{
    byte[] Render(InvoiceDocumentDto invoice, InvoicePdfLabels labels);
}

/// <summary>Localized, culture-resolved labels for the invoice PDF (passed in by the controller).</summary>
public sealed record InvoicePdfLabels(
    string Title,
    string Issuer,
    string InvoiceNumber,
    string IssuedAt,
    string BilledTo,
    string Plan,
    string BillingCycle,
    string Amount,
    string Total,
    string PaidNote
);

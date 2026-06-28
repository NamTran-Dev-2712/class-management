namespace ClassManagement.Domain.Modules.Payment.Enums;

// Supported payment gateways (MVP-8). Stored as PascalCase text; the webhook route parses the segment
// case-insensitively. See PaymentConfiguration check constraint.
public enum PaymentProvider
{
    Momo,
    VnPay,
}

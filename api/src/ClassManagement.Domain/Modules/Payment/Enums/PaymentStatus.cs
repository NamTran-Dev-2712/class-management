namespace ClassManagement.Domain.Modules.Payment.Enums;

// Payment order state (MVP-8). Pending → Completed / Failed / Expired (order timeout). Stored as
// PascalCase text; see PaymentConfiguration check constraint.
public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Expired,
}

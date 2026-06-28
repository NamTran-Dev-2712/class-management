namespace ClassManagement.Domain.Modules.Payment.Enums;

// How a subscription was created (MVP-8). Auto = paid via provider; Manual = Admin-granted (BR-8-08).
// Stored as PascalCase text; see SubscriptionConfiguration check constraint.
public enum SubscriptionPaymentType
{
    Auto,
    Manual,
}

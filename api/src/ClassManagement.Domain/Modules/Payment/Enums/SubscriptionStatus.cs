namespace ClassManagement.Domain.Modules.Payment.Enums;

// Subscription lifecycle state (MVP-8). Active → PastDue → Expired, or Active → Cancelled → Expired.
// Stored as PascalCase text; see SubscriptionConfiguration check constraint.
public enum SubscriptionStatus
{
    Active,
    PastDue,
    Cancelled,
    Expired,
}

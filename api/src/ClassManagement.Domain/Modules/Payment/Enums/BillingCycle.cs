namespace ClassManagement.Domain.Modules.Payment.Enums;

// Billing period of a paid plan (MVP-8). Stored as PascalCase text (matches the codebase enum-as-text
// convention); NULL on the Free plan. See PlanConfiguration / PaymentConfiguration check constraints.
public enum BillingCycle
{
    Monthly,
    Annual,
}

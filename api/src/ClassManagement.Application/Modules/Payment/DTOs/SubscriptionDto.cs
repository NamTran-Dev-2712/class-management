namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>
/// A teacher's current subscription (MVP-8). When there is no Active/PastDue subscription the teacher is
/// on Free and <see cref="PublicId"/> is null.
/// </summary>
public sealed record SubscriptionDto(
    Guid? PublicId,
    string PlanName,
    bool IsPro,
    string Status,
    string? BillingCycle,
    DateTime? StartedAt,
    DateTime? ExpiresAt,
    DateTime? CancelledAt,
    DateTime? GracePeriodEndsAt,
    string PaymentType
);

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// Read-model over <c>vw_subscriptions</c> (MVP-8): a subscription joined to its teacher and plan for the
/// admin list (A8-01). Enum-backed columns are exposed as plain strings (the column is text), as elsewhere.
/// </summary>
public sealed class SubscriptionView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long TeacherId { get; set; }
    public Guid? TeacherPublicId { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }

    public string PlanName { get; set; } = string.Empty;
    public long PlanPriceVnd { get; set; }
    public string? BillingCycle { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string? AdminNote { get; set; }

    public DateTime UpdatedAt { get; set; }
}

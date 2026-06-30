using ClassManagement.Domain.Modules.Payment.Enums;

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// A teacher's subscription to a <see cref="Plan"/> (MVP-8). At most one Active subscription per teacher
/// (partial unique index <c>uq_subscriptions_teacher_active</c>, BR-8-01). Mutable — <see cref="UpdatedAt"/>
/// via the <c>set_updated_at</c> trigger (the lifecycle sweep updates rows set-based, bypassing the
/// EF interceptor). No active row ⇒ the teacher is on Free (lazy, no seeded Free row per teacher).
/// </summary>
public sealed class Subscription : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long TeacherId { get; set; }
    public long PlanId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public DateTime StartedAt { get; set; }

    // NULL on a Free subscription (never expires); NOT NULL on Pro.
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // PastDue → Expired after this instant (= ExpiresAt + grace period).
    public DateTime? GracePeriodEndsAt { get; set; }

    public SubscriptionPaymentType PaymentType { get; set; } = SubscriptionPaymentType.Auto;

    // Set when an admin grants Pro manually (BR-8-08).
    public string? AdminNote { get; set; }

    public DateTime UpdatedAt { get; set; }
}

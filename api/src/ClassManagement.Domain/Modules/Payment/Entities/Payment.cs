using ClassManagement.Domain.Modules.Payment.Enums;

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// A payment order through a provider (MVP-8). <see cref="IdempotencyKey"/> (unique) dedups webhook
/// retries — the same key is applied exactly once. The plan price is snapshotted into
/// <see cref="AmountVnd"/> at order time (locked-in). Status mutates Pending → Completed/Failed/Expired;
/// the row has no <c>updated_at</c> — <see cref="CompletedAt"/> records the terminal time.
/// </summary>
public sealed class Payment : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    // NULL until the subscription is created/linked (webhook activation).
    public long? SubscriptionId { get; set; }
    public long TeacherId { get; set; }
    public long PlanId { get; set; }

    public PaymentProvider Provider { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public long AmountVnd { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string IdempotencyKey { get; set; } = string.Empty;
    public string? ProviderOrderId { get; set; }
    public string? ProviderTransactionId { get; set; }

    // Raw provider response (jsonb) for audit/debugging.
    public Dictionary<string, object?>? ProviderMetadata { get; set; }

    // Order timeout — Pending past this instant is swept to Expired.
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

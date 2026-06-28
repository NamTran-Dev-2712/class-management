using ClassManagement.Domain.Modules.Payment.Enums;

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// An immutable invoice issued automatically when a payment completes (MVP-8, BR-8-06). One-to-one with
/// a payment (<c>uq_invoices_payment</c>). Plan name + billing cycle are snapshotted (no FK to plans) so
/// later plan edits never change a historical invoice. Append-only — <see cref="BaseEntity.CreatedAt"/>
/// is mapped to the <c>issued_at</c> column; never updated or soft-deleted.
/// </summary>
public sealed class Invoice : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long PaymentId { get; set; }
    public long? SubscriptionId { get; set; }
    public long TeacherId { get; set; }

    // Format: INV-YYYYMM-###### (global sequence; see migration).
    public string InvoiceNumber { get; set; } = string.Empty;

    public long AmountVnd { get; set; }

    // Snapshots — not FKs.
    public string PlanName { get; set; } = string.Empty;
    public BillingCycle BillingCycle { get; set; }

    public string? PdfUrl { get; set; }
}

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// Read-model over <c>vw_payments</c> (MVP-8): a payment joined to its teacher and plan for the admin
/// payment history (A8-03). Enum-backed columns are exposed as plain strings (the column is text).
/// </summary>
public sealed class PaymentView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long TeacherId { get; set; }
    public Guid? TeacherPublicId { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }

    public string PlanName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public long AmountVnd { get; set; }
    public string Status { get; set; } = string.Empty;

    public string? ProviderTransactionId { get; set; }
    public DateTime? CompletedAt { get; set; }
}

using ClassManagement.Domain.Modules.Payment.Enums;

namespace ClassManagement.Domain.Modules.Payment.Entities;

/// <summary>
/// A subscription plan — Free or Pro (MVP-8). Seeded; admins may add more. Mutable (price/limits change
/// over time) so <see cref="UpdatedAt"/> is maintained by the <c>set_updated_at</c> trigger. A NULL
/// resource limit means unlimited. Changing a price never affects an existing Active subscription
/// (locked into <c>payments.amount_vnd</c>) — only new purchases use the new price (BR-8-07).
/// </summary>
public sealed class Plan : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public string Name { get; set; } = string.Empty;

    // NULL on the Free plan.
    public BillingCycle? BillingCycle { get; set; }

    public long PriceVnd { get; set; }

    // NULL = unlimited (enforced at the app layer, not a DB constraint).
    public int? MaxClasses { get; set; }
    public int? MaxQuestions { get; set; }
    public int? MaxExams { get; set; }
    public int? MaxStudentsPerClass { get; set; }

    // Total media storage cap in bytes (MVP-9). NULL = unlimited (e.g. Pro).
    public long? MaxStorageBytes { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Display-only feature labels for the pricing page (jsonb).
    public List<string>? Features { get; set; }

    public DateTime UpdatedAt { get; set; }
}

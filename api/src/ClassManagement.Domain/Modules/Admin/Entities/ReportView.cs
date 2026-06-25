namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// Read-model over <c>vw_reports</c> (MVP-7): a report joined to its reporter and reviewing admin for
/// display. Enum-backed columns are exposed as plain strings (the column is text), as elsewhere.
/// </summary>
public sealed class ReportView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }

    public long ReporterId { get; set; }
    public Guid? ReporterPublicId { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterEmail { get; set; }

    public string TargetType { get; set; } = string.Empty;
    public Guid? TargetPublicId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public long? AdminId { get; set; }
    public string? AdminName { get; set; }
    public string? AdminAction { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

namespace ClassManagement.Domain.Modules.Admin.Entities;

/// <summary>
/// A single tunable key-value system setting (MVP-7). <see cref="Value"/> is raw JSON stored in a jsonb
/// column; <see cref="ValueType"/> hints how to parse/render it. Read (cached) by
/// <c>ISystemSettingsService</c> to drive resource limits and other runtime config; written only by
/// Admin. <see cref="UpdatedAt"/> is maintained by the <c>set_updated_at</c> trigger.
/// </summary>
public sealed class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    /// <summary>Raw JSON text (jsonb column): a number, quoted string, boolean, or object.</summary>
    public string Value { get; set; } = "null";

    public string? Description { get; set; }

    /// <summary>One of string|integer|boolean|json — drives validation + UI rendering.</summary>
    public string ValueType { get; set; } = "string";

    /// <summary>When true the setting may be read by the frontend through a public endpoint.</summary>
    public bool IsPublic { get; set; }

    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

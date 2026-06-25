namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>One tunable system setting for the admin settings screen (MVP-7). <see cref="Value"/> is raw JSON.</summary>
public sealed record SystemSettingDto(
    string Key,
    string Value,
    string ValueType,
    bool IsPublic,
    string? Description,
    DateTime UpdatedAt
);

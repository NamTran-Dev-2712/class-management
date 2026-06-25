namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>One report row for the my-reports / admin-reports lists (MVP-7).</summary>
public sealed record ReportListDto(
    Guid PublicId,
    Guid? ReporterPublicId,
    string? ReporterName,
    string? ReporterEmail,
    string TargetType,
    Guid? TargetPublicId,
    string Reason,
    string? Description,
    string Status,
    string? AdminName,
    string? AdminAction,
    string? AdminNote,
    DateTime? ResolvedAt,
    DateTime CreatedAt
);

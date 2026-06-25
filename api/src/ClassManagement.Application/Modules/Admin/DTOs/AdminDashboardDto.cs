namespace ClassManagement.Application.Modules.Admin.DTOs;

/// <summary>A single day's count for a time-series chart (date as yyyy-MM-dd).</summary>
public sealed record DailyCountDto(string Date, int Count);

/// <summary>A status → count pair for a breakdown chart.</summary>
public sealed record StatusCountDto(string Status, int Count);

/// <summary>Overview counters + chart series for the admin dashboard (A7-01 / MVP-7.5).</summary>
public sealed record AdminDashboardDto(
    int TotalUsers,
    int NewUsersLast7Days,
    int ActiveClasses,
    int OpenAssignments,
    int PendingReports,
    IReadOnlyList<DailyCountDto> NewUsersDaily,
    IReadOnlyList<StatusCountDto> ReportsByStatus,
    IReadOnlyList<StatusCountDto> AssignmentsByStatus
);

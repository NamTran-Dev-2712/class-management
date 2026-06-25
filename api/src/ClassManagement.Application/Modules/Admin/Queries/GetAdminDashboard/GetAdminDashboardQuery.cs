using ClassManagement.Application.Modules.Admin.DTOs;
using ClassManagement.Domain.Modules.Admin.Enums;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;

// Admin dashboard overview counters + chart series (A7-01 / MVP-7.5). Aggregates from existing tables —
// no new view. The 30-day new-user series is gap-filled in memory so the chart has a point per day.
public record GetAdminDashboardQuery : IRequest<AdminDashboardDto>;

public class GetAdminDashboardQueryHandler
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private const int TrendDays = 30;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDirectory _userDirectory;

    public GetAdminDashboardQueryHandler(IUnitOfWork unitOfWork, IUserDirectory userDirectory)
    {
        _unitOfWork = unitOfWork;
        _userDirectory = userDirectory;
    }

    public async Task<AdminDashboardDto> Handle(
        GetAdminDashboardQuery request,
        CancellationToken ct
    )
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _userDirectory.CountUsersAsync(ct);
        var newUsers = await _userDirectory.CountUsersCreatedSinceAsync(now.AddDays(-7), ct);

        var activeClasses = await _unitOfWork
            .Repository<Class>()
            .CountAsync(c => c.Status == ClassStatus.Active);
        var openAssignments = await _unitOfWork
            .Repository<Assignment>()
            .CountAsync(a => a.Status == AssignmentStatus.Open);
        var pendingReports = await _unitOfWork
            .Repository<Report>()
            .CountAsync(r => r.Status == ReportStatus.Pending);

        var newUsersDaily = await BuildNewUserTrendAsync(now, ct);
        var reportsByStatus = await BuildReportsByStatusAsync(ct);
        var assignmentsByStatus = await BuildAssignmentsByStatusAsync(ct);

        return new AdminDashboardDto(
            totalUsers,
            newUsers,
            activeClasses,
            openAssignments,
            pendingReports,
            newUsersDaily,
            reportsByStatus,
            assignmentsByStatus
        );
    }

    private async Task<IReadOnlyList<DailyCountDto>> BuildNewUserTrendAsync(
        DateTime now,
        CancellationToken ct
    )
    {
        var startDate = DateOnly.FromDateTime(now.AddDays(-(TrendDays - 1)));
        // Npgsql requires a UTC-kind DateTime to compare against a timestamptz column.
        var since = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var counts = await _userDirectory.GetDailyNewUserCountsAsync(since, ct);

        var series = new List<DailyCountDto>(TrendDays);
        for (var i = 0; i < TrendDays; i++)
        {
            var day = startDate.AddDays(i);
            series.Add(
                new DailyCountDto(
                    day.ToString("yyyy-MM-dd"),
                    counts.TryGetValue(day, out var c) ? c : 0
                )
            );
        }
        return series;
    }

    private async Task<IReadOnlyList<StatusCountDto>> BuildReportsByStatusAsync(
        CancellationToken ct
    )
    {
        var repo = _unitOfWork.Repository<Report>();
        var rows = await repo.ToListAsync(
            repo.Query()
                .GroupBy(r => r.Status)
                .Select(g => new StatusGroup<ReportStatus>(g.Key, g.Count())),
            ct
        );
        var byStatus = rows.ToDictionary(r => r.Status, r => r.Count);
        return Enum.GetValues<ReportStatus>()
            .Select(s => new StatusCountDto(s.ToString(), byStatus.GetValueOrDefault(s)))
            .ToList();
    }

    private async Task<IReadOnlyList<StatusCountDto>> BuildAssignmentsByStatusAsync(
        CancellationToken ct
    )
    {
        var repo = _unitOfWork.Repository<Assignment>();
        var rows = await repo.ToListAsync(
            repo.Query()
                .GroupBy(a => a.Status)
                .Select(g => new StatusGroup<AssignmentStatus>(g.Key, g.Count())),
            ct
        );
        var byStatus = rows.ToDictionary(r => r.Status, r => r.Count);
        return Enum.GetValues<AssignmentStatus>()
            .Select(s => new StatusCountDto(s.ToString(), byStatus.GetValueOrDefault(s)))
            .ToList();
    }

    private readonly record struct StatusGroup<TStatus>(TStatus Status, int Count);
}

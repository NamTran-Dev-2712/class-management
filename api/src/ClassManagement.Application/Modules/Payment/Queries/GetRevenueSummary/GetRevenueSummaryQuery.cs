using System.Globalization;
using ClassManagement.Application.Modules.Payment.DTOs;

// Revenue summary for the admin dashboard (A8-04): total + by-month + by-plan over Completed payments
// within the trailing window (default 12 months). Aggregated in SQL over vw_payments.
public record GetRevenueSummaryQuery(int Months = 12) : IRequest<RevenueSummaryDto>;

public class GetRevenueSummaryQueryHandler
    : IRequestHandler<GetRevenueSummaryQuery, RevenueSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRevenueSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RevenueSummaryDto> Handle(
        GetRevenueSummaryQuery request,
        CancellationToken ct
    )
    {
        var months = Math.Clamp(request.Months, 1, 36);
        // Trailing window starting at the first day of the (months-1) earlier month, UTC.
        var firstOfThisMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );
        var since = firstOfThisMonth.AddMonths(-(months - 1));

        var repo = _unitOfWork.Repository<PaymentView>();
        var completed = repo.Query()
            .Where(p => p.Status == "Completed" && p.CompletedAt != null && p.CompletedAt >= since);

        // Group in SQL by year+month and by plan.
        var byMonthRaw = await repo.ToListAsync(
            completed
                .GroupBy(p => new { p.CompletedAt!.Value.Year, p.CompletedAt!.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.AmountVnd),
                    Count = g.Count(),
                }),
            ct
        );

        var byPlanRaw = await repo.ToListAsync(
            completed
                .GroupBy(p => p.PlanName)
                .Select(g => new
                {
                    PlanName = g.Key,
                    Revenue = g.Sum(x => x.AmountVnd),
                    Count = g.Count(),
                }),
            ct
        );

        var byMonth = byMonthRaw
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .Select(r => new RevenueByMonthDto(
                string.Format(CultureInfo.InvariantCulture, "{0:D4}-{1:D2}", r.Year, r.Month),
                r.Revenue,
                r.Count
            ))
            .ToList();

        var byPlan = byPlanRaw
            .OrderByDescending(r => r.Revenue)
            .Select(r => new RevenueByPlanDto(r.PlanName, r.Revenue, r.Count))
            .ToList();

        var total = byMonthRaw.Sum(r => r.Revenue);
        var count = byMonthRaw.Sum(r => r.Count);

        return new RevenueSummaryDto(total, count, byMonth, byPlan);
    }
}

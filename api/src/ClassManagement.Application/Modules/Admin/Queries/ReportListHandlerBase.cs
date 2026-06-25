using System.Linq.Expressions;
using ClassManagement.Application.Modules.Admin.DTOs;

// Shared filter surface for the report lists: target type + status, plus inherited paging/sort/search.
public abstract record ReportFilterQuery : BaseFilterQuery
{
    public ReportTargetType? TargetType { get; init; }
    public ReportStatus? Status { get; init; }
}

// Shared search/filter/sort/projection over vw_reports. Concrete handlers scope the rows (own / all).
public abstract class ReportListHandlerBase<TQuery>
    : BaseGetQueryHandler<TQuery, ReportView, ReportListDto>
    where TQuery : ReportFilterQuery, IRequest<PaginatedResult<ReportListDto>>
{
    protected ReportListHandlerBase(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<ReportView> ApplySearch(
        IQueryable<ReportView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(r =>
            (r.Description != null && r.Description.ToLower().Contains(keyword))
            || (r.ReporterName != null && r.ReporterName.ToLower().Contains(keyword))
            || (r.ReporterEmail != null && r.ReporterEmail.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<ReportView> ApplyFilter(
        IQueryable<ReportView> query,
        TQuery request
    )
    {
        if (request.TargetType.HasValue)
        {
            var type = request.TargetType.Value.ToString();
            query = query.Where(r => r.TargetType == type);
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(r => r.Status == status);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<ReportView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = r => r.CreatedAt,
            ["status"] = r => r.Status,
            ["targettype"] = r => r.TargetType,
        };

    protected override IQueryable<ReportListDto> ApplyProjection(IQueryable<ReportView> query) =>
        query.Select(r => new ReportListDto(
            r.PublicId,
            r.ReporterPublicId,
            r.ReporterName,
            r.ReporterEmail,
            r.TargetType,
            r.TargetPublicId,
            r.Reason,
            r.Description,
            r.Status,
            r.AdminName,
            r.AdminAction,
            r.AdminNote,
            r.ResolvedAt,
            r.CreatedAt
        ));
}

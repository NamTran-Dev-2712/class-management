using ClassManagement.Application.Modules.Admin.DTOs;

// Admin lists all reports across the platform (A7-02), filterable by target type + status.
public record GetAdminReportsQuery : ReportFilterQuery, IRequest<PaginatedResult<ReportListDto>>;

public class GetAdminReportsQueryHandler : ReportListHandlerBase<GetAdminReportsQuery>
{
    public GetAdminReportsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // No scope — admins see every report; the base pipeline handles search/filter/sort/paging.
}

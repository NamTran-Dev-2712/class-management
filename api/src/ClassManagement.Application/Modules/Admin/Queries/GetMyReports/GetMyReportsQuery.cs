using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Admin.DTOs;

// The current user's own submitted reports (MVP-7).
public record GetMyReportsQuery : ReportFilterQuery, IRequest<PaginatedResult<ReportListDto>>;

public class GetMyReportsQueryHandler : ReportListHandlerBase<GetMyReportsQuery>
{
    private readonly ICurrentUserService _currentUser;

    public GetMyReportsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork)
    {
        _currentUser = currentUser;
    }

    protected override IQueryable<ReportView> GetBaseQuery(IGenericRepository<ReportView> repo)
    {
        var userId =
            _currentUser.UserId ?? throw new UnauthorizedException("Auth.NotAuthenticated");
        return repo.Query().Where(r => r.ReporterId == userId);
    }
}

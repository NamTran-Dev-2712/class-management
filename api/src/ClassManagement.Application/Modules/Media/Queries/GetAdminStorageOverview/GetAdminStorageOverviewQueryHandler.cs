using ClassManagement.Application.Modules.Media.DTOs;

// Aggregates media bytes/count per teacher via the repository (grouping stays in Infrastructure to keep
// EF out of Application), then wraps the page in PaginatedResult.
public sealed class GetAdminStorageOverviewQueryHandler
    : IRequestHandler<GetAdminStorageOverviewQuery, PaginatedResult<StorageOverviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminStorageOverviewQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<StorageOverviewDto>> Handle(
        GetAdminStorageOverviewQuery request,
        CancellationToken cancellationToken
    )
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        var (rows, total) = await _unitOfWork.Media.GetStorageOverviewAsync(
            skip,
            request.PageSize,
            cancellationToken
        );

        return new PaginatedResult<StorageOverviewDto>(
            rows,
            total,
            request.PageNumber,
            request.PageSize
        );
    }
}

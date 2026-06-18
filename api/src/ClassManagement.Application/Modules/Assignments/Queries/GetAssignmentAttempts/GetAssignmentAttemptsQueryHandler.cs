using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetAssignmentAttemptsQueryHandler
    : IRequestHandler<GetAssignmentAttemptsQuery, PaginatedResult<AttemptListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAssignmentAttemptsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<AttemptListDto>> Handle(
        GetAssignmentAttemptsQuery request,
        CancellationToken ct
    )
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.AssignmentId, false, ct),
            _currentUser.UserId
        );

        var repo = _unitOfWork.Repository<AttemptView>();
        var query = repo.Query().Where(a => a.AssignmentId == assignment.Id);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(a => a.Status == status);
        }

        return await AttemptPaging.RunAsync(repo, query, request, ct);
    }
}

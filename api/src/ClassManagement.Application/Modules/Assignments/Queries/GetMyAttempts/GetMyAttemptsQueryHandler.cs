using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetMyAttemptsQueryHandler
    : IRequestHandler<GetMyAttemptsQuery, PaginatedResult<AttemptListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyAttemptsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<AttemptListDto>> Handle(
        GetMyAttemptsQuery request,
        CancellationToken ct
    )
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var repo = _unitOfWork.Repository<AttemptView>();
        var query = repo.Query().Where(a => a.StudentId == studentId);

        if (request.AssignmentId.HasValue)
            query = query.Where(a => a.AssignmentPublicId == request.AssignmentId);

        return await AttemptPaging.RunAsync(repo, query, request, ct);
    }
}

using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetMyAttemptResultQueryHandler
    : IRequestHandler<GetMyAttemptResultQuery, AttemptResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyAttemptResultQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AttemptResultDto> Handle(
        GetMyAttemptResultQuery request,
        CancellationToken ct
    )
    {
        var attempt = AssignmentGuard.EnsureAttemptOwner(
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct),
            _currentUser.UserId
        );

        return await AttemptResultLoader.BuildAsync(_unitOfWork, attempt, false, ct);
    }
}

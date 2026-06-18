using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetTeacherAttemptDetailQueryHandler
    : IRequestHandler<GetTeacherAttemptDetailQuery, AttemptResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetTeacherAttemptDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AttemptResultDto> Handle(
        GetTeacherAttemptDetailQuery request,
        CancellationToken ct
    )
    {
        var attempt =
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, true, ct)
            ?? throw new NotFoundException("Attempt.NotFound");

        // The teacher must own the assignment this attempt belongs to.
        var avRepo = _unitOfWork.Repository<AssignmentView>();
        var ownerId = (
            await avRepo.ToListAsync(
                avRepo
                    .Query()
                    .Where(a => a.Id == attempt.AssignmentId)
                    .Select(a => a.TeacherId)
                    .Take(1),
                ct
            )
        ).FirstOrDefault();

        if (ownerId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        return await AttemptResultLoader.BuildAsync(_unitOfWork, attempt, true, ct);
    }
}

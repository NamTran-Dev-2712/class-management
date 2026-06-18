using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetAssignmentDetailQueryHandler
    : IRequestHandler<GetAssignmentDetailQuery, AssignmentDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAssignmentDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AssignmentDetailDto> Handle(
        GetAssignmentDetailQuery request,
        CancellationToken ct
    )
    {
        var view =
            await AssignmentDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        if (view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        return await AssignmentDetailLoader.BuildDetailAsync(_unitOfWork, view, ct);
    }
}

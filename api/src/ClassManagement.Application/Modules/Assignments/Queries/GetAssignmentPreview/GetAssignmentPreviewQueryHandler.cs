using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetAssignmentPreviewQueryHandler
    : IRequestHandler<GetAssignmentPreviewQuery, AssignmentPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAssignmentPreviewQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AssignmentPreviewDto> Handle(
        GetAssignmentPreviewQuery request,
        CancellationToken ct
    )
    {
        var view =
            await AssignmentDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        if (view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        return await AssignmentDetailLoader.BuildPreviewAsync(_unitOfWork, view, ct);
    }
}

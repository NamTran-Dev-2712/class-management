using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;

public class GetAdminAssignmentDetailQueryHandler
    : IRequestHandler<GetAdminAssignmentDetailQuery, AssignmentDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminAssignmentDetailQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignmentDetailDto> Handle(
        GetAdminAssignmentDetailQuery request,
        CancellationToken ct
    )
    {
        var view =
            await AssignmentDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        return await AssignmentDetailLoader.BuildDetailAsync(_unitOfWork, view, ct);
    }
}

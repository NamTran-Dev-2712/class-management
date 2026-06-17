using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Exams.DTOs;

public class GetExamDetailQueryHandler : IRequestHandler<GetExamDetailQuery, ExamDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetExamDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ExamDetailDto> Handle(GetExamDetailQuery request, CancellationToken ct)
    {
        var view =
            await ExamDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Exam.NotFound");

        if (view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Exam.NotOwner");

        return await ExamDetailLoader.BuildDetailAsync(_unitOfWork, view, ct);
    }
}

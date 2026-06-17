using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Exams.DTOs;

public class GetExamPreviewQueryHandler : IRequestHandler<GetExamPreviewQuery, ExamPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetExamPreviewQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ExamPreviewDto> Handle(GetExamPreviewQuery request, CancellationToken ct)
    {
        var view =
            await ExamDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Exam.NotFound");

        if (view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Exam.NotOwner");

        return await ExamDetailLoader.BuildPreviewAsync(_unitOfWork, view, ct);
    }
}

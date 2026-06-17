using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Exams.DTOs;
using ClassManagement.Domain.Modules.Exams.Enums;

public class GetPublicExamDetailQueryHandler
    : IRequestHandler<GetPublicExamDetailQuery, ExamDetailDto>
{
    private static readonly string PublicVisibility = ExamVisibility.Public.ToString();
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPublicExamDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ExamDetailDto> Handle(GetPublicExamDetailQuery request, CancellationToken ct)
    {
        var view =
            await ExamDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Exam.NotFound");

        // Hide private exams of other teachers (404 — don't leak their existence).
        if (view.Visibility != PublicVisibility && view.TeacherId != _currentUser.UserId)
            throw new NotFoundException("Exam.NotFound");

        return await ExamDetailLoader.BuildDetailAsync(_unitOfWork, view, ct);
    }
}

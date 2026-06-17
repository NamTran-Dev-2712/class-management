using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Exams.DTOs;

public class GetAdminExamDetailQueryHandler
    : IRequestHandler<GetAdminExamDetailQuery, ExamDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminExamDetailQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ExamDetailDto> Handle(GetAdminExamDetailQuery request, CancellationToken ct)
    {
        var view =
            await ExamDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Exam.NotFound");

        return await ExamDetailLoader.BuildDetailAsync(_unitOfWork, view, ct);
    }
}

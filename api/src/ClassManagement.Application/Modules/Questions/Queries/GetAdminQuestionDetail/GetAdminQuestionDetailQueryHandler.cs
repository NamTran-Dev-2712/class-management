using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Questions.DTOs;

public class GetAdminQuestionDetailQueryHandler
    : IRequestHandler<GetAdminQuestionDetailQuery, QuestionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminQuestionDetailQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<QuestionDetailDto> Handle(
        GetAdminQuestionDetailQuery request,
        CancellationToken ct
    )
    {
        var view =
            await QuestionDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Question.NotFound");

        return await QuestionDetailLoader.BuildAsync(_unitOfWork, view, ct);
    }
}

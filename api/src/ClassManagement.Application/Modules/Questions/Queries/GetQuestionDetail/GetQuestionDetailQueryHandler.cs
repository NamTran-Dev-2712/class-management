using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Questions.DTOs;

public class GetQuestionDetailQueryHandler
    : IRequestHandler<GetQuestionDetailQuery, QuestionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetQuestionDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<QuestionDetailDto> Handle(
        GetQuestionDetailQuery request,
        CancellationToken ct
    )
    {
        var view =
            await QuestionDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Question.NotFound");

        if (view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Question.NotOwner");

        return await QuestionDetailLoader.BuildAsync(_unitOfWork, view, ct);
    }
}

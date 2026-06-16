using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

public class GetPublicQuestionDetailQueryHandler
    : IRequestHandler<GetPublicQuestionDetailQuery, QuestionDetailDto>
{
    private static readonly string PublicVisibility = QuestionVisibility.Public.ToString();
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPublicQuestionDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<QuestionDetailDto> Handle(
        GetPublicQuestionDetailQuery request,
        CancellationToken ct
    )
    {
        var view =
            await QuestionDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Question.NotFound");

        // Hide private questions of other teachers (404 — don't leak their existence).
        if (view.Visibility != PublicVisibility && view.TeacherId != _currentUser.UserId)
            throw new NotFoundException("Question.NotFound");

        return await QuestionDetailLoader.BuildAsync(_unitOfWork, view, ct);
    }
}

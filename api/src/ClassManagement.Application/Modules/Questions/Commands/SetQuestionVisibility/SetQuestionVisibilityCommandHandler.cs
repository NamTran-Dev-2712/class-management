public class SetQuestionVisibilityCommandHandler : IRequestHandler<SetQuestionVisibilityCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SetQuestionVisibilityCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(SetQuestionVisibilityCommand request, CancellationToken ct)
    {
        var question = QuestionGuard.EnsureOwned(
            await _unitOfWork.Questions.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        question.Visibility = request.Visibility;
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

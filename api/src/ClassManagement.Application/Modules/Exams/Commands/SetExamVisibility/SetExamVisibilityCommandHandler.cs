public class SetExamVisibilityCommandHandler : IRequestHandler<SetExamVisibilityCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SetExamVisibilityCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(SetExamVisibilityCommand request, CancellationToken ct)
    {
        var exam = ExamGuard.EnsureOwned(
            await _unitOfWork.Exams.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        exam.Visibility = request.Visibility;
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

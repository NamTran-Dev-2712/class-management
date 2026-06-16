public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteQuestionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken ct)
    {
        var question = QuestionGuard.EnsureOwned(
            await _unitOfWork.Questions.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // BR-3-08: a question used by ≥1 Exam cannot be deleted (must be removed from the Exam first).
        // Exams arrive in MVP-4; once exam_questions exists, add the in-use check here and throw
        // ConflictException("Question.InUseByExam") with the offending exams.

        // Soft-delete via the AuditableEntityInterceptor (ISoftDeletable → DeletedAt = now).
        _unitOfWork.Questions.Remove(question);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

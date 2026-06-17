using ClassManagement.Application.Exceptions;

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

        // BR-3-08 / BR-4-05: a question still used in one of the teacher's own (non-deleted) exams
        // cannot be soft-deleted — they must remove it from those exams first. (A Public question used
        // in *another* teacher's exam is not blocked; that exam shows it as "unavailable" instead.)
        var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();
        var examRepo = _unitOfWork.Repository<Exam>();
        var inUse = await examQuestionRepo.CountAsync(
            examQuestionRepo
                .Query()
                .Where(eq =>
                    eq.QuestionId == question.Id
                    && examRepo
                        .Query()
                        .Any(e => e.Id == eq.ExamId && e.TeacherId == question.TeacherId)
                ),
            ct
        );
        if (inUse > 0)
            throw new ConflictException("Question.InUseByExam");

        // Soft-delete via the AuditableEntityInterceptor (ISoftDeletable → DeletedAt = now).
        _unitOfWork.Questions.Remove(question);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

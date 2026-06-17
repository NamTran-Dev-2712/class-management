public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteExamCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteExamCommand request, CancellationToken ct)
    {
        var exam = ExamGuard.EnsureOwned(
            await _unitOfWork.Exams.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // BR-4-05: an exam used by ≥1 Assignment cannot be deleted (the teacher must be told which
        // assignments still use it). Assignments arrive in MVP-5; once that table exists, add the
        // in-use check here and throw ConflictException("Exam.InUseByAssignment").

        // Soft-delete via the AuditableEntityInterceptor (ISoftDeletable → DeletedAt = now). The
        // exam_questions rows stay (FK cascade only fires on a hard delete) — fine, they are hidden
        // with the parent and re-deleted on a future hard purge.
        _unitOfWork.Exams.Remove(exam);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

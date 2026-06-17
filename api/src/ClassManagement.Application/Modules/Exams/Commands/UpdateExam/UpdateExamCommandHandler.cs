using ClassManagement.Application.Exceptions;

public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateExamCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateExamCommand request, CancellationToken ct)
    {
        var exam = ExamGuard.EnsureOwned(
            await _unitOfWork.Exams.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Subject is optional. When provided, resolve it (active unless it's the one already attached,
        // so an existing-but-deactivated subject can be kept); when omitted, detach the subject.
        long? subjectId = null;
        if (request.SubjectId is Guid subjectPublicId)
        {
            var subject =
                await _unitOfWork.Subjects.GetByPublicIdAsync(subjectPublicId, ct)
                ?? throw new NotFoundException("Subject.NotFound");
            if (!subject.IsActive && subject.Id != exam.SubjectId)
                throw new BadException("Exam.SubjectInactive");
            subjectId = subject.Id;
        }

        exam.SubjectId = subjectId;
        exam.Title = request.Title.Trim();
        exam.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        exam.Visibility = request.Visibility;

        // Metadata-only change → version is intentionally left untouched.
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

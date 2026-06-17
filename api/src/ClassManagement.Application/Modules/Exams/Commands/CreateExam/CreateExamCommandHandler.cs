using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Exams.Enums;

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IExamPolicy _policy;

    public CreateExamCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IExamPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<Guid> Handle(CreateExamCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        // BR-4-09: optional cap on exams per teacher (0 = unlimited).
        if (_policy.MaxExamsPerTeacher > 0)
        {
            var count = await _unitOfWork.Exams.CountByTeacherAsync(teacherId, ct);
            if (count >= _policy.MaxExamsPerTeacher)
                throw new BadException("Exam.MaxExamsReached");
        }

        // Subject is optional. When chosen it must resolve and be active.
        long? subjectId = null;
        if (request.SubjectId is Guid subjectPublicId)
        {
            var subject =
                await _unitOfWork.Subjects.GetByPublicIdAsync(subjectPublicId, ct)
                ?? throw new NotFoundException("Subject.NotFound");
            if (!subject.IsActive)
                throw new BadException("Exam.SubjectInactive");
            subjectId = subject.Id;
        }

        var entity = new Exam
        {
            TeacherId = teacherId,
            SubjectId = subjectId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Visibility = request.Visibility,
            Version = 1,
            TotalPoint = 0,
            TotalQuestions = 0,
        };

        await _unitOfWork.Exams.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return entity.PublicId;
    }
}

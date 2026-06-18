using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAssignmentPolicy _policy;

    public CreateAssignmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAssignmentPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        if (_policy.MaxAssignmentsPerTeacher > 0)
        {
            var count = await _unitOfWork.Assignments.CountByTeacherAsync(teacherId, ct);
            if (count >= _policy.MaxAssignmentsPerTeacher)
                throw new BadException("Assignment.MaxAssignmentsReached");
        }

        // The exam must be owned by this teacher and have at least one scored question (BR-5-01).
        var exam = ExamGuard.EnsureOwned(
            await _unitOfWork.Exams.GetByPublicIdAsync(request.ExamId, false, ct),
            teacherId
        );
        if (exam.TotalQuestions <= 0 || exam.TotalPoint <= 0)
            throw new BadException("Assignment.ExamHasNoQuestions");

        // The class must be owned by this teacher.
        var cls = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassId, ct),
            teacherId
        );

        var entity = new Assignment
        {
            ExamId = exam.Id,
            ClassId = cls.Id,
            TeacherId = teacherId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            OpensAt = request.OpensAt,
            ClosesAt = request.ClosesAt,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            ScorePolicy = request.ScorePolicy,
            AllowLate = request.AllowLate,
            GradePublishPolicy = request.GradePublishPolicy,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleOptions = request.ShuffleOptions,
            ShowAnswersAfterGrade = request.ShowAnswersAfterGrade,
            Status = AssignmentStatus.Draft,
        };

        await _unitOfWork.Assignments.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return entity.PublicId;
    }
}

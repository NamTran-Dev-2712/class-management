using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class GetStudentAssignmentDetailQueryHandler
    : IRequestHandler<GetStudentAssignmentDetailQuery, StudentAssignmentDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetStudentAssignmentDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<StudentAssignmentDetailDto> Handle(
        GetStudentAssignmentDetailQuery request,
        CancellationToken ct
    )
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var repo = _unitOfWork.Repository<StudentAssignmentView>();
        var view =
            (
                await repo.ToListAsync(
                    repo.Query()
                        .Where(a => a.PublicId == request.PublicId && a.StudentId == studentId)
                        .Take(1),
                    ct
                )
            ).FirstOrDefault() ?? throw new NotFoundException("Assignment.NotFound");

        var now = DateTime.UtcNow;
        var attemptsLeft = Math.Max(0, view.MaxAttempts - view.UsedAttempts);
        var isOpen =
            view.Status == AssignmentStatus.Open.ToString()
            && (view.OpensAt is null || view.OpensAt <= now)
            && (view.ClosesAt is null || view.ClosesAt >= now);
        var canStart = isOpen && (view.HasInProgress || attemptsLeft > 0);

        return new StudentAssignmentDetailDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            Description = view.Description,
            Status = view.Status,
            OpensAt = view.OpensAt,
            ClosesAt = view.ClosesAt,
            TimeLimitMinutes = view.TimeLimitMinutes,
            MaxAttempts = view.MaxAttempts,
            AllowLate = view.AllowLate,
            GradePublishPolicy = view.GradePublishPolicy,
            ClassPublicId = view.ClassPublicId,
            ClassName = view.ClassName,
            TeacherName = view.TeacherName,
            TotalPoint = view.TotalPoint,
            TotalQuestions = view.TotalQuestions,
            UsedAttempts = view.UsedAttempts,
            AttemptsLeft = attemptsLeft,
            HasInProgress = view.HasInProgress,
            InProgressAttemptPublicId = view.InProgressAttemptPublicId,
            CanStart = canStart,
        };
    }
}

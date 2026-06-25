using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Exams.Enums;

public class DuplicateExamCommandHandler : IRequestHandler<DuplicateExamCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IExamPolicy _policy;

    public DuplicateExamCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IExamPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<Guid> Handle(DuplicateExamCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var source = ExamGuard.EnsureReadable(
            await _unitOfWork.Exams.GetByPublicIdAsync(
                request.PublicId,
                includeQuestions: true,
                includeTags: true,
                cancellationToken: ct
            ),
            teacherId
        );

        var maxExams = await _policy.GetMaxExamsPerTeacherAsync(ct);
        if (maxExams > 0)
        {
            var count = await _unitOfWork.Exams.CountByTeacherAsync(teacherId, ct);
            if (count >= maxExams)
                throw new BadException("Exam.MaxExamsReached");
        }

        var copy = new Exam
        {
            TeacherId = teacherId,
            SubjectId = source.SubjectId,
            Title = source.Title,
            Description = source.Description,
            Visibility = ExamVisibility.Private,
            Version = 1,
            TotalPoint = source.TotalPoint,
            TotalQuestions = source.TotalQuestions,
            Questions = source
                .Questions.OrderBy(q => q.DisplayOrder)
                .Select(q => new ExamQuestion
                {
                    QuestionId = q.QuestionId,
                    DisplayOrder = q.DisplayOrder,
                    Point = q.Point,
                })
                .ToList(),
            Tags = source.Tags.Select(t => new ExamTag { Tag = t.Tag }).ToList(),
        };

        await _unitOfWork.Exams.AddAsync(copy, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return copy.PublicId;
    }
}

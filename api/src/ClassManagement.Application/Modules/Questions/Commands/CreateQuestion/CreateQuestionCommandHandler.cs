using ClassManagement.Application.Exceptions;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceLimitService _resourceLimits;

    public CreateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IResourceLimitService resourceLimits
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _resourceLimits = resourceLimits;
    }

    public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        // BR-3-11 + MVP-8: effective cap = the teacher's plan limit (Pro = unlimited) or the live Free
        // system_settings cap (0 = unlimited).
        var maxQuestions = await _resourceLimits.GetMaxQuestionsAsync(teacherId, ct);
        if (maxQuestions > 0)
        {
            var count = await _unitOfWork.Questions.CountByTeacherAsync(teacherId, ct);
            if (count >= maxQuestions)
                throw new BadException("Question.MaxQuestionsReached");
        }

        // Subject is optional (BR-3-01 relaxed: a teacher may create questions before any catalog
        // subject exists). When a subject is chosen it must resolve and be active.
        long? subjectId = null;
        if (request.SubjectId is Guid subjectPublicId)
        {
            var subject =
                await _unitOfWork.Subjects.GetByPublicIdAsync(subjectPublicId, ct)
                ?? throw new NotFoundException("Subject.NotFound");
            if (!subject.IsActive)
                throw new BadException("Question.SubjectInactive");
            subjectId = subject.Id;
        }

        var entity = new Question
        {
            TeacherId = teacherId,
            SubjectId = subjectId,
            Type = request.Type,
            Content = request.Content.Trim(),
            Difficulty = request.Difficulty,
            SuggestedPoint = request.SuggestedPoint,
            Visibility = request.Visibility,
            Explanation = string.IsNullOrWhiteSpace(request.Explanation)
                ? null
                : request.Explanation.Trim(),
            Options = QuestionAssembler.BuildOptions(request.Type, request.Options),
            Tags = QuestionAssembler.BuildTags(request.Tags),
        };

        await _unitOfWork.Questions.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return entity.PublicId;
    }
}

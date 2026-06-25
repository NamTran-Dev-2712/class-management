using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Questions.Enums;

public class DuplicateQuestionCommandHandler : IRequestHandler<DuplicateQuestionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IQuestionPolicy _policy;

    public DuplicateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IQuestionPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<Guid> Handle(DuplicateQuestionCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var source = QuestionGuard.EnsureReadable(
            await _unitOfWork.Questions.GetByPublicIdAsync(request.PublicId, true, ct),
            teacherId
        );

        var maxQuestions = await _policy.GetMaxQuestionsPerTeacherAsync(ct);
        if (maxQuestions > 0)
        {
            var count = await _unitOfWork.Questions.CountByTeacherAsync(teacherId, ct);
            if (count >= maxQuestions)
                throw new BadException("Question.MaxQuestionsReached");
        }

        var copy = new Question
        {
            TeacherId = teacherId,
            SubjectId = source.SubjectId,
            Type = source.Type,
            Content = source.Content,
            Difficulty = source.Difficulty,
            SuggestedPoint = source.SuggestedPoint,
            Visibility = QuestionVisibility.Private,
            Explanation = source.Explanation,
            Options = source
                .Options.OrderBy(o => o.DisplayOrder)
                .Select(o => new QuestionOption
                {
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                })
                .ToList(),
            Tags = source.Tags.Select(t => new QuestionTag { Tag = t.Tag }).ToList(),
        };

        await _unitOfWork.Questions.AddAsync(copy, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return copy.PublicId;
    }
}

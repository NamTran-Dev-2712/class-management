using ClassManagement.Application.Exceptions;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateQuestionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateQuestionCommand request, CancellationToken ct)
    {
        var question = QuestionGuard.EnsureOwned(
            await _unitOfWork.Questions.GetByPublicIdAsync(request.PublicId, true, ct),
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
            if (!subject.IsActive && subject.Id != question.SubjectId)
                throw new BadException("Question.SubjectInactive");
            subjectId = subject.Id;
        }

        question.SubjectId = subjectId;
        question.Type = request.Type;
        question.Content = request.Content.Trim();
        question.Difficulty = request.Difficulty;
        question.SuggestedPoint = request.SuggestedPoint;
        question.Visibility = request.Visibility;
        question.Explanation = string.IsNullOrWhiteSpace(request.Explanation)
            ? null
            : request.Explanation.Trim();

        // Resolve any referenced media (option images + attachments) to internal ids, enforcing
        // owner + Confirmed (BR-9-01/07).
        var mediaMap = await QuestionMediaResolver.ResolveAsync(
            _unitOfWork,
            _currentUser.UserId!.Value,
            QuestionAssembler.CollectMediaPublicIds(
                request.Type,
                request.Options,
                request.Attachments
            ),
            ct
        );

        // Replace child collections wholesale — removed rows cascade-delete (required FK + cascade).
        question.Options.Clear();
        foreach (
            var option in QuestionAssembler.BuildOptions(request.Type, request.Options, mediaMap)
        )
            question.Options.Add(option);

        question.Tags.Clear();
        foreach (var tag in QuestionAssembler.BuildTags(request.Tags))
            question.Tags.Add(tag);

        question.Media.Clear();
        foreach (var media in QuestionAssembler.BuildMedia(request.Attachments, mediaMap))
            question.Media.Add(media);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}

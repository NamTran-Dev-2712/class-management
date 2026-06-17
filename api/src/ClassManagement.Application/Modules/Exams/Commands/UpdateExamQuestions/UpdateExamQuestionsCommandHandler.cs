using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Questions.Enums;

public class UpdateExamQuestionsCommandHandler : IRequestHandler<UpdateExamQuestionsCommand>
{
    private static readonly string PublicVisibility = QuestionVisibility.Public.ToString();
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateExamQuestionsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateExamQuestionsCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId;
        var exam = ExamGuard.EnsureOwned(
            await _unitOfWork.Exams.GetByPublicIdAsync(request.PublicId, true, ct),
            teacherId
        );

        // Resolve the referenced questions in one query and verify each is readable by this teacher
        // (their own, or any Public question). Soft-deleted questions are excluded by the global query
        // filter, so a deleted reference resolves to "not found" and is rejected here.
        var requested = request.Questions;
        var publicIds = requested.Select(q => q.QuestionId).Distinct().ToList();

        var questionRepo = _unitOfWork.Repository<Question>();
        var resolved = await questionRepo.ToListAsync(
            questionRepo
                .Query()
                .Where(q => publicIds.Contains(q.PublicId))
                .Select(q => new QuestionRef(q.Id, q.PublicId, q.TeacherId, q.Visibility)),
            ct
        );

        var byPublicId = resolved.ToDictionary(q => q.PublicId);
        foreach (var publicId in publicIds)
        {
            if (!byPublicId.TryGetValue(publicId, out var q))
                throw new BadException("Exam.QuestionNotFound");

            if (q.Visibility != QuestionVisibility.Public && q.TeacherId != teacherId)
                throw new BadException("Exam.QuestionNotFound");
        }

        // Replace the question set wholesale — removed rows cascade-delete (FK + cascade). Display
        // order is 1-based from list position.
        exam.Questions.Clear();
        var order = 1;
        foreach (var item in requested)
        {
            exam.Questions.Add(
                new ExamQuestion
                {
                    QuestionId = byPublicId[item.QuestionId].Id,
                    DisplayOrder = order++,
                    Point = item.Point,
                }
            );
        }

        // Recompute the denormalized caches and bump the version (app-layer maintained; one commit).
        exam.TotalQuestions = requested.Count;
        exam.TotalPoint = requested.Sum(q => q.Point);
        exam.Version += 1;

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private readonly record struct QuestionRef(
        long Id,
        Guid PublicId,
        long TeacherId,
        QuestionVisibility Visibility
    );
}

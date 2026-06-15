using ClassManagement.Application.Exceptions;

public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateClassCommand request, CancellationToken ct)
    {
        var entity = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.PublicId, ct),
            _currentUser.UserId
        );

        var name = request.Name.Trim();
        if (
            await _unitOfWork.Classes.NameExistsForOwnerAsync(
                name,
                entity.OwnerId,
                request.PublicId,
                ct
            )
        )
            throw new ConflictException("Class.NameExists");

        var (subjectId, subjectName) = await ClassSubjectResolver.ResolveAsync(
            _unitOfWork,
            request.SubjectId,
            request.SubjectName,
            ct
        );

        entity.Name = name;
        entity.Description = request.Description?.Trim();
        entity.SubjectId = subjectId;
        entity.SubjectName = subjectName;
        entity.CoverImageUrl = request.CoverImageUrl?.Trim();

        _unitOfWork.Classes.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

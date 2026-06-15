using ClassManagement.Domain.Modules.Classroom.Enums;

public class ArchiveClassCommandHandler : IRequestHandler<ArchiveClassCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ArchiveClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ArchiveClassCommand request, CancellationToken ct)
    {
        var entity = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.PublicId, ct),
            _currentUser.UserId
        );

        entity.Status = request.Archive ? ClassStatus.Archived : ClassStatus.Active;

        _unitOfWork.Classes.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

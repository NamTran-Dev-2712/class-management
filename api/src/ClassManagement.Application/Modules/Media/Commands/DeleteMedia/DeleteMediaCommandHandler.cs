using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Identity;

// Soft-deletes a media asset. Owner or Admin (BR-9-07); the ISoftDeletable interceptor turns Remove into
// DeletedAt = now. The RESTRICT FK from question_media is not tripped (the row survives the soft-delete),
// and a published snapshot's frozen copy is untouched (BR-9-06).
public sealed class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteMediaCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var isAdmin = string.Equals(
            _currentUser.Role,
            ApplicationRoles.Admin,
            StringComparison.Ordinal
        );

        var asset = MediaGuard.EnsureOwnedOrAdmin(
            await _unitOfWork.Media.GetByPublicIdAsync(request.PublicId, cancellationToken),
            _currentUser.UserId,
            isAdmin
        );

        _unitOfWork.Media.Remove(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

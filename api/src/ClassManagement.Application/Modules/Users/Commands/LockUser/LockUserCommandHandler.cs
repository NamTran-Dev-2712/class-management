public class LockUserCommandHandler : IRequestHandler<LockUserCommand>
{
    private readonly IUserAdminRepository _users;
    private readonly ICurrentUserService _currentUser;

    public LockUserCommandHandler(IUserAdminRepository users, ICurrentUserService currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task Handle(LockUserCommand request, CancellationToken ct)
    {
        await UserGuard.EnsureManageableAsync(_users, request.PublicId, ct);
        await _users.SetLockAsync(request.PublicId, true, _currentUser.UserId, ct);
    }
}

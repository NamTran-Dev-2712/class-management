public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand>
{
    private readonly IUserAdminRepository _users;

    public UnlockUserCommandHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public async Task Handle(UnlockUserCommand request, CancellationToken ct)
    {
        await UserGuard.EnsureManageableAsync(_users, request.PublicId, ct);
        await _users.SetLockAsync(request.PublicId, false, null, ct);
    }
}

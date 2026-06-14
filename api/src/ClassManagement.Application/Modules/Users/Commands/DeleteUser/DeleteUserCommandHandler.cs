public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserAdminRepository _users;

    public DeleteUserCommandHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        await UserGuard.EnsureManageableAsync(_users, request.PublicId, ct);
        await _users.SoftDeleteAsync(request.PublicId, ct);
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserAdminRepository _users;

    public UpdateUserCommandHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        await UserGuard.EnsureManageableAsync(_users, request.PublicId, ct);

        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        await _users.UpdateAsync(
            request.PublicId,
            request.DisplayName.Trim(),
            request.Role,
            phoneNumber,
            ct
        );
    }
}

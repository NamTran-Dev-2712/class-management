public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto>
{
    private readonly IUserAdminRepository _users;

    public GetUserByIdQueryHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public Task<UserDetailDto> Handle(GetUserByIdQuery request, CancellationToken ct) =>
        UserGuard.EnsureManageableAsync(_users, request.PublicId, ct);
}

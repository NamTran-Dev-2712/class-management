public record GetUsersQuery : BaseFilterQuery, IRequest<PaginatedResult<UserDto>>
{
    public bool? IsLocked { get; init; }
    public string? Role { get; init; }
}

using System.Linq.Expressions;
using ClassManagement.Application.Common.Constants;

public class GetUsersQueryHandler : BaseGetQueryHandler<GetUsersQuery, User, UserDto>
{
    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // User management is scoped to Students/Teachers — admin accounts (including the current
    // admin) are never listed. Applied to the base query so search/filter/COUNT all respect it.
    protected override IQueryable<User> GetBaseQuery(IGenericRepository<User> repo) =>
        repo.Query().Where(u => !u.Roles.Contains(ApplicationRoles.Admin));

    protected override IQueryable<User> ApplySearch(IQueryable<User> query, string searchTerm)
    {
        var keyword = searchTerm.ToLower();
        return query.Where(u =>
            u.DisplayName.ToLower().Contains(keyword) || u.Email.ToLower().Contains(keyword)
        );
    }

    protected override IQueryable<User> ApplyFilter(IQueryable<User> query, GetUsersQuery request)
    {
        if (request.IsLocked.HasValue)
            query = query.Where(u => u.IsLocked == request.IsLocked.Value);

        if (!string.IsNullOrWhiteSpace(request.Role))
            query = query.Where(u => u.Roles.Contains(request.Role));

        return query;
    }

    protected override Dictionary<string, Expression<Func<User, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [UserSortKeys.DisplayName] = u => u.DisplayName,
            [UserSortKeys.Email] = u => u.Email,
            [UserSortKeys.LastLoginAt] = u => u.LastLoginAt!,
            [UserSortKeys.CreatedAt] = u => u.CreatedAt,
        };

    protected override IQueryable<UserDto> ApplyProjection(IQueryable<User> query) =>
        query.Select(u => new UserDto
        {
            PublicId = u.PublicId,
            DisplayName = u.DisplayName,
            Email = u.Email,
            EmailConfirmed = u.EmailConfirmed,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            IsLocked = u.IsLocked,
            LockedAt = u.LockedAt,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            Roles = u.Roles,
        });
}

public static class UserSortKeys
{
    public const string DisplayName = "displayName";
    public const string Email = "email";
    public const string LastLoginAt = "lastLoginAt";
    public const string CreatedAt = "createdAt";
}

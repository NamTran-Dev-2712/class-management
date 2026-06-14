using ClassManagement.Application.Common.Constants;
using SortOrderValues = ClassManagement.Application.Common.Constants.SortOrder;

namespace ClassManagement.Application.Common.Pagination;

/// <summary>
/// Base for paginated list/filter queries. Reusable across modules — derive a query
/// record from this and pair it with <see cref="BaseGetQueryHandler{TQuery,TEntity,TDto}"/>.
/// </summary>
public abstract record BaseFilterQuery
{
    public int PageNumber { get; init; } = PaginationValue.DefaultPage;

    private readonly int _pageSize = PaginationValue.DefaultPageSize;

    /// <summary>Clamped to [1, MaxPageSize] to prevent unbounded result sets.</summary>
    public int PageSize
    {
        get => _pageSize;
        init =>
            _pageSize =
                value < 1 ? PaginationValue.DefaultPageSize
                : value > PaginationValue.MaxPageSize ? PaginationValue.MaxPageSize
                : value;
    }

    public string? SortBy { get; init; }
    public string SortOrder { get; init; } = SortOrderValues.Ascending;
    public string? SearchTerm { get; init; }
}

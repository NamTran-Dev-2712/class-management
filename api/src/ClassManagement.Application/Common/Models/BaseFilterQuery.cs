using ClassManagement.Application.Common.Constants;

namespace ClassManagement.Application.Common.Models;

public abstract class BaseFilterQuery
{
    private int _pageSize = PaginationValue.DefaultPageSize;

    public int PageNumber { get; init; } = PaginationValue.DefaultPage;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, 1, PaginationValue.MaxPageSize);
    }

    public string? SortBy { get; init; }
    public string SortOrder { get; init; } = Constants.SortOrder.Descending;
    public string? Search { get; init; }
}

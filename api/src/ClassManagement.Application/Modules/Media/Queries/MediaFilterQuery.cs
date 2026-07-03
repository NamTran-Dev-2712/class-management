using ClassManagement.Application.Modules.Media.DTOs;

// Shared filter surface for media list queries (MVP-9). Search matches content type; filters by kind /
// status (and owner for the admin surface).
public abstract record MediaFilterQuery : BaseFilterQuery, IRequest<PaginatedResult<MediaListDto>>
{
    public MediaKind? Kind { get; init; }
    public MediaStatus? Status { get; init; }
}

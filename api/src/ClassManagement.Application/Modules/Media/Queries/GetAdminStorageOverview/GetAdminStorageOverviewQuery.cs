using ClassManagement.Application.Modules.Media.DTOs;

// Per-teacher storage aggregate for the admin overview, ordered by usage descending (MVP-9, A9-01).
public sealed record GetAdminStorageOverviewQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<StorageOverviewDto>>;

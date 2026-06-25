using ClassManagement.Application.Modules.Admin.DTOs;

// Admin audit log search (MVP-7): filter by action, target type, actor (public id) and a UTC date range,
// plus the inherited paging/sort/search. Default sort is newest first (BaseGetQueryHandler).
public record GetAuditLogsQuery : BaseFilterQuery, IRequest<PaginatedResult<AuditLogListDto>>
{
    public string? Action { get; init; }
    public string? TargetType { get; init; }
    public Guid? ActorPublicId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
}

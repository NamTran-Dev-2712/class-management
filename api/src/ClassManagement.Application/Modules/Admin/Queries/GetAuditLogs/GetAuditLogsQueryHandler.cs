using System.Linq.Expressions;
using ClassManagement.Application.Modules.Admin.DTOs;

// Lists audit logs for Admin oversight over vw_audit_logs. Search matches action/actor name/email;
// filters narrow by action, target type, actor, and date range. EF-free (plain LINQ).
public class GetAuditLogsQueryHandler
    : BaseGetQueryHandler<GetAuditLogsQuery, AuditLogView, AuditLogListDto>
{
    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<AuditLogView> ApplySearch(
        IQueryable<AuditLogView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(a =>
            a.Action.ToLower().Contains(keyword)
            || (a.ActorName != null && a.ActorName.ToLower().Contains(keyword))
            || (a.ActorEmail != null && a.ActorEmail.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<AuditLogView> ApplyFilter(
        IQueryable<AuditLogView> query,
        GetAuditLogsQuery request
    )
    {
        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (!string.IsNullOrWhiteSpace(request.TargetType))
            query = query.Where(a => a.TargetType == request.TargetType);

        if (request.ActorPublicId.HasValue)
            query = query.Where(a => a.ActorPublicId == request.ActorPublicId);

        if (request.FromUtc.HasValue)
            query = query.Where(a => a.CreatedAt >= request.FromUtc.Value);

        if (request.ToUtc.HasValue)
            query = query.Where(a => a.CreatedAt <= request.ToUtc.Value);

        return query;
    }

    protected override Dictionary<string, Expression<Func<AuditLogView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = a => a.CreatedAt,
            ["action"] = a => a.Action,
        };

    protected override IQueryable<AuditLogListDto> ApplyProjection(
        IQueryable<AuditLogView> query
    ) =>
        query.Select(a => new AuditLogListDto(
            a.Action,
            a.ActorPublicId,
            a.ActorName,
            a.ActorEmail,
            a.ActorRole,
            a.TargetType,
            a.TargetPublicId,
            a.Metadata,
            a.IpAddress,
            a.CreatedAt
        ));
}

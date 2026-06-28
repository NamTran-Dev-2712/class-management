using System.Linq.Expressions;
using ClassManagement.Application.Modules.Payment.DTOs;

// Admin lists every subscription (A8-01), filterable by status, searchable by teacher. Over vw_subscriptions.
public record GetAdminSubscriptionsQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<AdminSubscriptionDto>>
{
    public SubscriptionStatus? Status { get; init; }
}

public class GetAdminSubscriptionsQueryHandler
    : BaseGetQueryHandler<GetAdminSubscriptionsQuery, SubscriptionView, AdminSubscriptionDto>
{
    public GetAdminSubscriptionsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<SubscriptionView> ApplySearch(
        IQueryable<SubscriptionView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(s =>
            (s.TeacherName != null && s.TeacherName.ToLower().Contains(keyword))
            || (s.TeacherEmail != null && s.TeacherEmail.ToLower().Contains(keyword))
        );
    }

    protected override IQueryable<SubscriptionView> ApplyFilter(
        IQueryable<SubscriptionView> query,
        GetAdminSubscriptionsQuery request
    )
    {
        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(s => s.Status == status);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<SubscriptionView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = s => s.CreatedAt,
            ["expiresat"] = s => s.ExpiresAt!,
            ["status"] = s => s.Status,
            ["planname"] = s => s.PlanName,
        };

    protected override IQueryable<AdminSubscriptionDto> ApplyProjection(
        IQueryable<SubscriptionView> query
    ) =>
        query.Select(s => new AdminSubscriptionDto(
            s.PublicId,
            s.TeacherPublicId,
            s.TeacherName,
            s.TeacherEmail,
            s.PlanName,
            s.PlanPriceVnd > 0,
            s.BillingCycle,
            s.Status,
            s.StartedAt,
            s.ExpiresAt,
            s.CancelledAt,
            s.GracePeriodEndsAt,
            s.PaymentType,
            s.AdminNote,
            s.CreatedAt
        ));
}

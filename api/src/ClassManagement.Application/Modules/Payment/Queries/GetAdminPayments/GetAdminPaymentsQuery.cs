using System.Linq.Expressions;
using ClassManagement.Application.Modules.Payment.DTOs;

// Admin payment history (A8-03), filterable by status + provider, searchable by teacher. Over vw_payments.
public record GetAdminPaymentsQuery : BaseFilterQuery, IRequest<PaginatedResult<AdminPaymentDto>>
{
    public PaymentStatus? Status { get; init; }
    public PaymentProvider? Provider { get; init; }
}

public class GetAdminPaymentsQueryHandler
    : BaseGetQueryHandler<GetAdminPaymentsQuery, PaymentView, AdminPaymentDto>
{
    public GetAdminPaymentsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    protected override IQueryable<PaymentView> ApplySearch(
        IQueryable<PaymentView> query,
        string searchTerm
    )
    {
        var keyword = searchTerm.ToLower();
        return query.Where(p =>
            (p.TeacherName != null && p.TeacherName.ToLower().Contains(keyword))
            || (p.TeacherEmail != null && p.TeacherEmail.ToLower().Contains(keyword))
            || (
                p.ProviderTransactionId != null
                && p.ProviderTransactionId.ToLower().Contains(keyword)
            )
        );
    }

    protected override IQueryable<PaymentView> ApplyFilter(
        IQueryable<PaymentView> query,
        GetAdminPaymentsQuery request
    )
    {
        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(p => p.Status == status);
        }

        if (request.Provider.HasValue)
        {
            var provider = request.Provider.Value.ToString();
            query = query.Where(p => p.Provider == provider);
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<PaymentView, object>>> SortKeyMap =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdat"] = p => p.CreatedAt,
            ["amountvnd"] = p => p.AmountVnd,
            ["status"] = p => p.Status,
        };

    protected override IQueryable<AdminPaymentDto> ApplyProjection(IQueryable<PaymentView> query) =>
        query.Select(p => new AdminPaymentDto(
            p.PublicId,
            p.TeacherPublicId,
            p.TeacherName,
            p.TeacherEmail,
            p.PlanName,
            p.Provider,
            p.BillingCycle,
            p.AmountVnd,
            p.Status,
            p.ProviderTransactionId,
            p.CreatedAt,
            p.CompletedAt
        ));
}

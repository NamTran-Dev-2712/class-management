using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;

// The current teacher's invoice history (MVP-8 T8-07). Newest first. Output-cached per user (InvoicesRead).
public record GetMyInvoicesQuery : IRequest<IReadOnlyList<InvoiceDto>>;

public class GetMyInvoicesQueryHandler
    : IRequestHandler<GetMyInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyInvoicesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InvoiceDto>> Handle(
        GetMyInvoicesQuery request,
        CancellationToken ct
    )
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");
        var invoices = await _unitOfWork.Invoices.ListByTeacherAsync(teacherId, ct);
        return invoices
            .Select(i => new InvoiceDto(
                i.PublicId,
                i.InvoiceNumber,
                i.AmountVnd,
                i.PlanName,
                i.BillingCycle.ToString(),
                i.CreatedAt
            ))
            .ToList();
    }
}

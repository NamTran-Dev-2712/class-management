using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;

// Resolves a single invoice for PDF download (MVP-8 T8-03). Guarded: the owning teacher, or any admin.
// The controller renders the PDF from the returned document with localized labels.
public record GetInvoiceForDownloadQuery(Guid InvoicePublicId) : IRequest<InvoiceDocumentDto>;

public class GetInvoiceForDownloadQueryHandler
    : IRequestHandler<GetInvoiceForDownloadQuery, InvoiceDocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetInvoiceForDownloadQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<InvoiceDocumentDto> Handle(
        GetInvoiceForDownloadQuery request,
        CancellationToken ct
    )
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var invoice =
            await _unitOfWork.Invoices.GetByPublicIdAsync(request.InvoicePublicId, ct)
            ?? throw new NotFoundException("Invoice.NotFound");

        var isAdmin = string.Equals(
            _currentUser.Role,
            ApplicationRoles.Admin,
            StringComparison.Ordinal
        );
        if (invoice.TeacherId != userId && !isAdmin)
            throw new ForbiddenException("Invoice.Forbidden");

        return new InvoiceDocumentDto(
            invoice.InvoiceNumber,
            invoice.CreatedAt,
            invoice.PlanName,
            invoice.BillingCycle.ToString(),
            invoice.AmountVnd,
            _currentUser.Email ?? string.Empty
        );
    }
}

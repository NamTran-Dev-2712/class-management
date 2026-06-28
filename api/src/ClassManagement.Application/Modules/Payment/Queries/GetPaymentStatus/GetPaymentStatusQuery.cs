using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;

// Current status of one of the teacher's own payment orders (MVP-8). Backs the checkout page's polling
// fallback when a webhook is delayed. Time-sensitive — never output-cached.
public record GetPaymentStatusQuery(Guid PaymentPublicId) : IRequest<PaymentStatusDto>;

public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var payment =
            await _unitOfWork.Payments.GetByPublicIdAsync(request.PaymentPublicId, ct)
            ?? throw new NotFoundException("Payment.NotFound");
        if (payment.TeacherId != teacherId)
            throw new ForbiddenException("Payment.Forbidden");

        return new PaymentStatusDto(
            payment.PublicId,
            payment.Provider.ToString(),
            payment.Status.ToString(),
            payment.AmountVnd,
            payment.BillingCycle.ToString(),
            payment.CreatedAt,
            payment.CompletedAt
        );
    }
}

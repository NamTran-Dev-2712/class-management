using ClassManagement.Application.Modules.Payment.DTOs;

// Active plans for the public pricing page (MVP-8). Anonymous-readable; ordered by display_order then
// price. Output-cached at the HTTP layer (PlansRead).
public record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPlansQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Plans.GetActiveOrderedAsync(ct);
        return plans
            .Select(p => new PlanDto(
                p.PublicId,
                p.Name,
                p.BillingCycle?.ToString(),
                p.PriceVnd,
                p.MaxClasses,
                p.MaxQuestions,
                p.MaxExams,
                p.MaxStudentsPerClass,
                p.IsActive,
                p.DisplayOrder,
                p.Features ?? []
            ))
            .ToList();
    }
}

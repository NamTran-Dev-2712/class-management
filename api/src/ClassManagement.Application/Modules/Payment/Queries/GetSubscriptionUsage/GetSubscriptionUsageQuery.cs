using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Payment.DTOs;

// The current teacher's resource usage vs. their effective plan limits (MVP-8). Drives the "X/Y used"
// UI and the upgrade prompt.
public record GetSubscriptionUsageQuery : IRequest<ResourceUsageDto>;

public class GetSubscriptionUsageQueryHandler
    : IRequestHandler<GetSubscriptionUsageQuery, ResourceUsageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceLimitService _resourceLimits;

    public GetSubscriptionUsageQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IResourceLimitService resourceLimits
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _resourceLimits = resourceLimits;
    }

    public async Task<ResourceUsageDto> Handle(
        GetSubscriptionUsageQuery request,
        CancellationToken ct
    )
    {
        var teacherId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var limits = await _resourceLimits.GetEffectiveLimitsAsync(teacherId, ct);
        var classes = await _unitOfWork.Classes.CountByOwnerAsync(teacherId, ct);
        var questions = await _unitOfWork.Questions.CountByTeacherAsync(teacherId, ct);
        var exams = await _unitOfWork.Exams.CountByTeacherAsync(teacherId, ct);

        return new ResourceUsageDto(
            limits.PlanName,
            limits.IsPro,
            new ResourceUsageItem(classes, limits.MaxClasses),
            new ResourceUsageItem(questions, limits.MaxQuestions),
            new ResourceUsageItem(exams, limits.MaxExams)
        );
    }
}

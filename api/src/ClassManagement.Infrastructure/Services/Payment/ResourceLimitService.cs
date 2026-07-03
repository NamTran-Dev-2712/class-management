namespace ClassManagement.Infrastructure.Services.Payment;

// Effective resource caps for a teacher (MVP-8). Active subscription → its plan's limits (null = unlimited);
// otherwise the live system_settings caps via the existing policies. Scoped + memoized per request so the
// usage endpoint and a create-handler in the same request don't re-query the subscription/plan.
public sealed class ResourceLimitService : IResourceLimitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassroomPolicy _classroomPolicy;
    private readonly IQuestionPolicy _questionPolicy;
    private readonly IExamPolicy _examPolicy;
    private readonly IMediaPolicy _mediaPolicy;

    private EffectiveResourceLimits? _cached;
    private long _cachedTeacherId;

    public ResourceLimitService(
        IUnitOfWork unitOfWork,
        IClassroomPolicy classroomPolicy,
        IQuestionPolicy questionPolicy,
        IExamPolicy examPolicy,
        IMediaPolicy mediaPolicy
    )
    {
        _unitOfWork = unitOfWork;
        _classroomPolicy = classroomPolicy;
        _questionPolicy = questionPolicy;
        _examPolicy = examPolicy;
        _mediaPolicy = mediaPolicy;
    }

    public async Task<EffectiveResourceLimits> GetEffectiveLimitsAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    )
    {
        if (_cached is not null && _cachedTeacherId == teacherId)
            return _cached;

        // Active or PastDue (grace) both grant Pro (BR-8-05).
        var subscription = await _unitOfWork.Subscriptions.GetEntitlingByTeacherAsync(
            teacherId,
            cancellationToken
        );
        if (subscription is not null)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(subscription.PlanId, cancellationToken);
            if (plan is not null)
            {
                _cachedTeacherId = teacherId;
                return _cached = new EffectiveResourceLimits(
                    plan.Name,
                    IsPro: plan.PriceVnd > 0,
                    MaxClasses: plan.MaxClasses ?? 0,
                    MaxQuestions: plan.MaxQuestions ?? 0,
                    MaxExams: plan.MaxExams ?? 0,
                    MaxStorageBytes: plan.MaxStorageBytes ?? 0
                );
            }
        }

        // Free tier: live system_settings caps (MVP-7 behavior preserved — admins tune Free caps live).
        _cachedTeacherId = teacherId;
        return _cached = new EffectiveResourceLimits(
            PlanName: "Free",
            IsPro: false,
            MaxClasses: await _classroomPolicy.GetMaxClassesPerTeacherAsync(cancellationToken),
            MaxQuestions: await _questionPolicy.GetMaxQuestionsPerTeacherAsync(cancellationToken),
            MaxExams: await _examPolicy.GetMaxExamsPerTeacherAsync(cancellationToken),
            MaxStorageBytes: await _mediaPolicy.GetFreeStorageQuotaBytesAsync(cancellationToken)
        );
    }

    public async Task<int> GetMaxClassesAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => (await GetEffectiveLimitsAsync(teacherId, cancellationToken)).MaxClasses;

    public async Task<int> GetMaxQuestionsAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => (await GetEffectiveLimitsAsync(teacherId, cancellationToken)).MaxQuestions;

    public async Task<int> GetMaxExamsAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => (await GetEffectiveLimitsAsync(teacherId, cancellationToken)).MaxExams;

    public async Task<long> GetMaxStorageBytesAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    ) => (await GetEffectiveLimitsAsync(teacherId, cancellationToken)).MaxStorageBytes;
}

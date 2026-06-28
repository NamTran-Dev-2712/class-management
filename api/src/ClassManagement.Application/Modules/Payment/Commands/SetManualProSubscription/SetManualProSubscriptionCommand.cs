using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;

// Admin grants a teacher a Pro subscription manually (BR-8-08, A8-02) — e.g. partnership / demo. Creates
// or overrides the teacher's Active subscription with payment_type='Manual' and an admin note; audited.
public record SetManualProSubscriptionCommand(
    Guid TeacherPublicId,
    Guid PlanPublicId,
    string? AdminNote
) : IRequest;

public class SetManualProSubscriptionCommandHandler
    : IRequestHandler<SetManualProSubscriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDirectory _userDirectory;
    private readonly IAuditLogger _audit;

    public SetManualProSubscriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IUserDirectory userDirectory,
        IAuditLogger audit
    )
    {
        _unitOfWork = unitOfWork;
        _userDirectory = userDirectory;
        _audit = audit;
    }

    public async Task Handle(SetManualProSubscriptionCommand request, CancellationToken ct)
    {
        var teacherId =
            await _userDirectory.GetUserIdByPublicIdAsync(request.TeacherPublicId, ct)
            ?? throw new NotFoundException("User.NotFound");

        var plan =
            await _unitOfWork.Plans.GetByPublicIdAsync(request.PlanPublicId, ct)
            ?? throw new NotFoundException("Plan.NotFound");
        if (!plan.IsActive)
            throw new BadException("Plan.Inactive");

        var now = DateTime.UtcNow;
        var note = string.IsNullOrWhiteSpace(request.AdminNote) ? null : request.AdminNote.Trim();
        var expiresAt = ExpiryFor(now, plan.BillingCycle);

        // Reuse the teacher's entitling (Active/PastDue) subscription if any — keeps one row (BR-8-01).
        var existing = await _unitOfWork.Subscriptions.GetEntitlingByTeacherAsync(teacherId, ct);
        if (existing is not null)
        {
            existing.PlanId = plan.Id;
            existing.Status = SubscriptionStatus.Active;
            existing.ExpiresAt = expiresAt;
            existing.CancelledAt = null;
            existing.GracePeriodEndsAt = null;
            existing.PaymentType = SubscriptionPaymentType.Manual;
            existing.AdminNote = note;
            _unitOfWork.Subscriptions.Update(existing);
        }
        else
        {
            existing = new Subscription
            {
                TeacherId = teacherId,
                PlanId = plan.Id,
                Status = SubscriptionStatus.Active,
                StartedAt = now,
                ExpiresAt = expiresAt,
                PaymentType = SubscriptionPaymentType.Manual,
                AdminNote = note,
            };
            await _unitOfWork.Subscriptions.AddAsync(existing, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAndSaveAsync(
            new AuditEntry
            {
                Action = AuditActions.SubscriptionManualSet,
                TargetType = AuditTargetTypes.Subscription,
                TargetId = existing.Id,
                TargetPublicId = existing.PublicId,
                Metadata = new Dictionary<string, object?>
                {
                    ["planName"] = plan.Name,
                    ["teacherPublicId"] = request.TeacherPublicId,
                    ["note"] = note,
                },
            },
            ct
        );
    }

    // Pro plans carry a billing cycle → expiry; a cycle-less (Free) plan never expires.
    private static DateTime? ExpiryFor(DateTime from, BillingCycle? cycle) =>
        cycle switch
        {
            BillingCycle.Annual => from.AddYears(1),
            BillingCycle.Monthly => from.AddMonths(1),
            _ => null,
        };
}

using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IInviteCodeGenerator _inviteCodes;
    private readonly IClassroomPolicy _policy;
    private readonly IResourceLimitService _resourceLimits;

    public CreateClassCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IInviteCodeGenerator inviteCodes,
        IClassroomPolicy policy,
        IResourceLimitService resourceLimits
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _inviteCodes = inviteCodes;
        _policy = policy;
        _resourceLimits = resourceLimits;
    }

    public async Task<Guid> Handle(CreateClassCommand request, CancellationToken ct)
    {
        var ownerId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");
        var name = request.Name.Trim();

        // BR-2-09 + MVP-8: effective cap = the teacher's plan limit (Pro = unlimited) or the live Free
        // system_settings cap (0 = unlimited).
        var maxClasses = await _resourceLimits.GetMaxClassesAsync(ownerId, ct);
        if (maxClasses > 0)
        {
            var count = await _unitOfWork.Classes.CountByOwnerAsync(ownerId, ct);
            if (count >= maxClasses)
                throw new BadException("Class.MaxClassesReached");
        }

        // Class name unique per teacher among non-deleted classes (uq_classes_name_owner_active).
        if (await _unitOfWork.Classes.NameExistsForOwnerAsync(name, ownerId, cancellationToken: ct))
            throw new ConflictException("Class.NameExists");

        var (subjectId, subjectName) = await ClassSubjectResolver.ResolveAsync(
            _unitOfWork,
            request.SubjectId,
            request.SubjectName,
            ct
        );

        var inviteCode = await AllocateInviteCodeAsync(ct);

        var entity = new Class
        {
            Name = name,
            Description = request.Description?.Trim(),
            SubjectId = subjectId,
            SubjectName = subjectName,
            OwnerId = ownerId,
            InviteCode = inviteCode,
            Status = ClassStatus.Active,
            CoverImageUrl = request.CoverImageUrl?.Trim(),
        };

        await _unitOfWork.Classes.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return entity.PublicId;
    }

    private async Task<string> AllocateInviteCodeAsync(CancellationToken ct)
    {
        var length = await _policy.GetInviteCodeLengthAsync(ct);
        string code;
        do
        {
            code = _inviteCodes.Generate(length);
        } while (await _unitOfWork.Classes.InviteCodeExistsAsync(code, ct));
        return code;
    }
}

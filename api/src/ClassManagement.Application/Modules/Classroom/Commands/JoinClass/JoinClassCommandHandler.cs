using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class JoinClassCommandHandler : IRequestHandler<JoinClassCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public JoinClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(JoinClassCommand request, CancellationToken ct)
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        // Invite codes are stored uppercase [A-Z0-9]; normalize the user's input.
        var code = request.InviteCode.Trim().ToUpperInvariant();

        var cls =
            await _unitOfWork.Classes.GetByInviteCodeAsync(code, ct)
            ?? throw new NotFoundException("Class.InviteCodeInvalid");

        if (cls.Status == ClassStatus.Archived)
            throw new BadException("Class.ArchivedCannotJoin");

        var existing = await _unitOfWork.ClassMemberships.GetActiveForStudentAsync(
            cls.Id,
            studentId,
            ct
        );
        if (existing is not null)
        {
            throw existing.Status == MembershipStatus.Approved
                ? new ConflictException("Class.AlreadyMember")
                : new ConflictException("Class.AlreadyPending");
        }

        var membership = new ClassMembership
        {
            ClassId = cls.Id,
            StudentId = studentId,
            Status = MembershipStatus.Pending,
            JoinedAt = DateTime.UtcNow,
        };

        await _unitOfWork.ClassMemberships.AddAsync(membership, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return cls.PublicId;
    }
}

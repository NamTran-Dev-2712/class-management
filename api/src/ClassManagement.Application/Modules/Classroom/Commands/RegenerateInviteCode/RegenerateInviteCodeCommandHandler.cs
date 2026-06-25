public class RegenerateInviteCodeCommandHandler
    : IRequestHandler<RegenerateInviteCodeCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IInviteCodeGenerator _inviteCodes;
    private readonly IClassroomPolicy _policy;

    public RegenerateInviteCodeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IInviteCodeGenerator inviteCodes,
        IClassroomPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _inviteCodes = inviteCodes;
        _policy = policy;
    }

    public async Task<string> Handle(RegenerateInviteCodeCommand request, CancellationToken ct)
    {
        var entity = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.PublicId, ct),
            _currentUser.UserId
        );

        var length = await _policy.GetInviteCodeLengthAsync(ct);
        string code;
        do
        {
            code = _inviteCodes.Generate(length);
        } while (await _unitOfWork.Classes.InviteCodeExistsAsync(code, ct));

        entity.InviteCode = code;
        _unitOfWork.Classes.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return code;
    }
}

public class RegenerateInviteCodeCommandHandler
    : IRequestHandler<RegenerateInviteCodeCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IInviteCodeGenerator _inviteCodes;

    public RegenerateInviteCodeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IInviteCodeGenerator inviteCodes
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _inviteCodes = inviteCodes;
    }

    public async Task<string> Handle(RegenerateInviteCodeCommand request, CancellationToken ct)
    {
        var entity = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.PublicId, ct),
            _currentUser.UserId
        );

        string code;
        do
        {
            code = _inviteCodes.Generate();
        } while (await _unitOfWork.Classes.InviteCodeExistsAsync(code, ct));

        entity.InviteCode = code;
        _unitOfWork.Classes.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return code;
    }
}

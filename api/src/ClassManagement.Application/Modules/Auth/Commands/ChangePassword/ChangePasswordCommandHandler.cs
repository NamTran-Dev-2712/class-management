using ClassManagement.Application.Exceptions;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthRepository _authRepository;

    public ChangePasswordCommandHandler(
        ICurrentUserService currentUser,
        IAuthRepository authRepository
    )
    {
        _currentUser = currentUser;
        _authRepository = authRepository;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        await _authRepository.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken
        );
    }
}

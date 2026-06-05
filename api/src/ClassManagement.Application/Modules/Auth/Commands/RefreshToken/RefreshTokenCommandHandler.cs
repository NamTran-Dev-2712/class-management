public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IAuthRepository _authRepository;

    public RefreshTokenCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public Task<AuthResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    ) =>
        _authRepository.RefreshTokenAsync(
            request.RawToken,
            request.IpAddress,
            request.UserAgent,
            cancellationToken
        );
}

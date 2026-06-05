public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenHasher _tokenHasher;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHasher tokenHasher
    )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenHasher = tokenHasher;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RawRefreshToken))
            return; // No token — cookies will still be cleared by controller

        var hash = _tokenHasher.Hash(request.RawRefreshToken);
        await _refreshTokenRepository.RevokeByHashAsync(hash, cancellationToken);
    }
}

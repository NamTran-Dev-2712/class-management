public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IAuthRepository _authRepository;

    public ResetPasswordCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
        _authRepository.ResetPasswordAsync(
            request.Email,
            request.Otp,
            request.NewPassword,
            cancellationToken
        );
}

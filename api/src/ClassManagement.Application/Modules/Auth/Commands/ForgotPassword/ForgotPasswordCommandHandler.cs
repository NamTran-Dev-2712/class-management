public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IAuthRepository _authRepository;

    public ForgotPasswordCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken) =>
        _authRepository.ForgotPasswordAsync(request.Email, cancellationToken);
}

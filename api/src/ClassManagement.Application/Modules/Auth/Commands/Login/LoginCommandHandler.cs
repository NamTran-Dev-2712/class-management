using MediatR;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthRepository _authRepository;

    public LoginCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResult = await _authRepository.LoginAsync(request.Email, request.Password);

        return authResult;
    }
}

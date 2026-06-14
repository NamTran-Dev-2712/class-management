using MediatR;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, long>
{
    private readonly IAuthRepository _authRepository;

    public RegisterCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<long> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await _authRepository.RegisterAsync(
            request.DisplayName,
            request.Email,
            request.Password,
            request.Role,
            cancellationToken
        );

        return userId;
    }
}

using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Interfaces.Messaging;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserAdminRepository _users;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailQueueService _emailQueue;

    public CreateUserCommandHandler(
        IUserAdminRepository users,
        IPasswordGenerator passwordGenerator,
        IEmailQueueService emailQueue
    )
    {
        _users = users;
        _passwordGenerator = passwordGenerator;
        _emailQueue = emailQueue;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        if (await _users.EmailExistsAsync(email, ct))
            throw new ConflictException("User.EmailAlreadyExists");

        // The admin never supplies a password — generate one and email it to the new user.
        var password = _passwordGenerator.Generate();
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        var publicId = await _users.CreateAsync(
            displayName,
            email,
            request.Role,
            phoneNumber,
            password,
            ct
        );

        _emailQueue.EnqueueWelcomeEmail(email, displayName, password);

        return publicId;
    }
}

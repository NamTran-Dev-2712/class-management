public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;

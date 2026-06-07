public record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;

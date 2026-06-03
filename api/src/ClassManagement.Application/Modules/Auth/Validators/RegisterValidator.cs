using System.Text.RegularExpressions;
using ClassManagement.Application.Common.Constants;
using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(50)
            .WithMessage("Display name must not exceed 50 characters.")
            .Must(NotContainDangerousCharacters)
            .WithMessage("Display name contains invalid characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Username is required.")
            .Length(3, 30)
            .WithMessage("Username must be between 3 and 30 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .WithMessage("Password is too long.")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required.")
            .Must(BeAnAllowedRole)
            .WithMessage("Invalid role specified.");
    }

    private bool NotContainDangerousCharacters(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return true;
        // prevent XSS by disallowing characters that are commonly used in HTML tags and attributes
        return !displayName.Contains("<")
            && !displayName.Contains(">")
            && !displayName.Contains("&");
    }

    private bool BeAnAllowedRole(string role)
    {
        return ApplicationRoles.All.Contains(role);
    }
}

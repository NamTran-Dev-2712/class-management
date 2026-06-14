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
            .WithMessage("Validation.DisplayName.Required")
            .MinimumLength(2)
            .WithMessage("Validation.DisplayName.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.DisplayName.MaxLength")
            .Must(NotContainDangerousCharacters)
            .WithMessage("Validation.DisplayName.Invalid");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.Email.Required")
            .MaximumLength(256)
            .WithMessage("Validation.Email.MaxLength")
            .EmailAddress()
            .WithMessage("Validation.Email.Invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Validation.Password.Required")
            .MinimumLength(8)
            .WithMessage("Validation.Password.MinLength")
            .MaximumLength(100)
            .WithMessage("Validation.Password.MaxLength")
            .Matches(@"[A-Z]")
            .WithMessage("Validation.Password.Uppercase")
            .Matches(@"[a-z]")
            .WithMessage("Validation.Password.Lowercase")
            .Matches(@"[0-9]")
            .WithMessage("Validation.Password.Number")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Validation.Password.Special");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Validation.Role.Required")
            .Must(BeSelfRegisterable)
            .WithMessage("Validation.Role.Invalid");
    }

    // Self-registration is limited to Student/Teacher — Admin is provisioned internally.
    private static readonly string[] SelfRegisterableRoles =
    [
        ApplicationRoles.Student,
        ApplicationRoles.Teacher,
    ];

    private bool BeSelfRegisterable(string role) => SelfRegisterableRoles.Contains(role);

    private bool NotContainDangerousCharacters(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return true;
        // prevent XSS by disallowing characters that are commonly used in HTML tags and attributes
        return !displayName.Contains("<")
            && !displayName.Contains(">")
            && !displayName.Contains("&");
    }
}

using ClassManagement.Application.Common.Constants;

public sealed class SendSystemAnnouncementValidator
    : AbstractValidator<SendSystemAnnouncementCommand>
{
    public SendSystemAnnouncementValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Validation.Announcement.TitleRequired")
            .MaximumLength(200)
            .WithMessage("Validation.Announcement.TitleTooLong");

        RuleFor(x => x.Body)
            .MaximumLength(500)
            .WithMessage("Validation.Announcement.BodyTooLong")
            .When(x => !string.IsNullOrEmpty(x.Body));

        RuleFor(x => x.Role)
            .Must(role => ApplicationRoles.All.Contains(role!))
            .WithMessage("Validation.Announcement.InvalidRole")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));
    }
}

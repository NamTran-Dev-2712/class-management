public sealed class UpdateSystemSettingValidator : AbstractValidator<UpdateSystemSettingCommand>
{
    public UpdateSystemSettingValidator()
    {
        RuleFor(x => x.Key).NotEmpty().WithMessage("Validation.SystemSetting.KeyRequired");
        RuleFor(x => x.Value).NotNull().WithMessage("Validation.SystemSetting.ValueRequired");
    }
}

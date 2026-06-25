using System.Globalization;
using System.Text.Json;
using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Admin.Constants;

// Admin updates one system setting (A7-06 / BR-7-06). Value is the human-entered value; the handler
// normalizes it to JSON per the setting's value_type, audits old→new, and invalidates the cache so the
// change takes effect live.
public record UpdateSystemSettingCommand(string Key, string Value) : IRequest;

public class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly ISystemSettingsService _settings;

    public UpdateSystemSettingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger,
        ISystemSettingsService settings
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _settings = settings;
    }

    public async Task Handle(UpdateSystemSettingCommand request, CancellationToken ct)
    {
        var setting =
            await _unitOfWork.SystemSettings.GetByKeyAsync(request.Key, ct)
            ?? throw new NotFoundException("SystemSetting.NotFound");

        var newValue = NormalizeToJson(setting.ValueType, request.Value);
        var oldValue = setting.Value;

        setting.Value = newValue;
        setting.UpdatedBy = _currentUser.UserId;
        _unitOfWork.SystemSettings.Update(setting);

        await _auditLogger.LogAsync(
            new AuditEntry
            {
                Action = AuditActions.AdminSystemSettingsChanged,
                TargetType = AuditTargetTypes.SystemSetting,
                Metadata = new Dictionary<string, object?>
                {
                    ["key"] = setting.Key,
                    ["old_value"] = oldValue,
                    ["new_value"] = newValue,
                },
            },
            ct
        );

        await _unitOfWork.SaveChangesAsync(ct);
        await _settings.InvalidateAsync(ct);
    }

    // Convert a human-entered value into the JSON form stored in the jsonb column, per value_type.
    private static string NormalizeToJson(string valueType, string value)
    {
        var trimmed = value.Trim();
        switch (valueType)
        {
            case SystemSettingValueTypes.Integer:
                if (
                    !long.TryParse(
                        trimmed,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var n
                    )
                )
                    throw new BadException("SystemSetting.InvalidValue");
                return n.ToString(CultureInfo.InvariantCulture);

            case SystemSettingValueTypes.Boolean:
                if (!bool.TryParse(trimmed, out var b))
                    throw new BadException("SystemSetting.InvalidValue");
                return b ? "true" : "false";

            case SystemSettingValueTypes.Json:
                try
                {
                    using var _ = JsonDocument.Parse(trimmed);
                    return trimmed;
                }
                catch (JsonException)
                {
                    throw new BadException("SystemSetting.InvalidValue");
                }

            default: // string
                return JsonSerializer.Serialize(value);
        }
    }
}

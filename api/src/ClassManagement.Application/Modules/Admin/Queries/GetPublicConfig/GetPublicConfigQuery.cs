using ClassManagement.Application.Modules.Admin.DTOs;
using ClassManagement.Domain.Modules.Admin.Constants;

// Public app config consumed by the frontend (brand name + maintenance flag). Anonymous-readable; only
// is_public settings are exposed. Read (cached) via ISystemSettingsService.
public record GetPublicConfigQuery : IRequest<PublicConfigDto>;

public class GetPublicConfigQueryHandler : IRequestHandler<GetPublicConfigQuery, PublicConfigDto>
{
    private const string DefaultAppName = "Class Management";

    private readonly ISystemSettingsService _settings;

    public GetPublicConfigQueryHandler(ISystemSettingsService settings)
    {
        _settings = settings;
    }

    public async Task<PublicConfigDto> Handle(GetPublicConfigQuery request, CancellationToken ct)
    {
        var appName =
            await _settings.GetStringAsync(SystemSettingKeys.AppName, DefaultAppName, ct)
            ?? DefaultAppName;
        var maintenance = await _settings.GetBoolAsync(
            SystemSettingKeys.MaintenanceMode,
            false,
            ct
        );
        return new PublicConfigDto(appName, maintenance);
    }
}

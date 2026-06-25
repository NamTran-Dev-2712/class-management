using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Classroom;

// Per-teacher class cap: system_settings is the live source of truth (MVP-7); the appsettings value is
// the fallback when the key is missing/unparsable.
public sealed class ClassroomPolicy : IClassroomPolicy
{
    private readonly ClassroomOptions _options;
    private readonly ISystemSettingsService _settings;

    public ClassroomPolicy(IOptions<ClassroomOptions> options, ISystemSettingsService settings)
    {
        _options = options.Value;
        _settings = settings;
    }

    public Task<int> GetMaxClassesPerTeacherAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.MaxClassesPerTeacher,
            _options.MaxClassesPerTeacher,
            cancellationToken
        );

    public Task<int> GetMaxStudentsPerClassAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.MaxStudentsPerClass,
            _options.MaxStudentsPerClass,
            cancellationToken
        );

    public async Task<int> GetInviteCodeLengthAsync(CancellationToken cancellationToken = default)
    {
        var length = await _settings.GetIntAsync(
            SystemSettingKeys.InviteCodeLength,
            _options.InviteCodeLength,
            cancellationToken
        );
        // Clamp to a safe range — too short is brute-forceable, too long is unwieldy.
        return Math.Clamp(length, 6, 12);
    }
}

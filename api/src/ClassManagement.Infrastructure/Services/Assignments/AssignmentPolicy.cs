using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Assignments;

// Exposes assignment tunables to Application handlers. Static caps come from appsettings; the live global
// attempts ceiling is read (cached) from system_settings with the appsettings cap as fallback (MVP-7.5).
public sealed class AssignmentPolicy : IAssignmentPolicy
{
    private readonly AssignmentOptions _options;
    private readonly ISystemSettingsService _settings;

    public AssignmentPolicy(IOptions<AssignmentOptions> options, ISystemSettingsService settings)
    {
        _options = options.Value;
        _settings = settings;
    }

    public int MaxAssignmentsPerTeacher => _options.MaxAssignmentsPerTeacher;
    public int MaxTimeLimitMinutes => _options.MaxTimeLimitMinutes;
    public int MaxAttemptsCap => _options.MaxAttemptsCap;
    public int LifecycleBatchSize => _options.LifecycleBatchSize;
    public int ReportHistogramBuckets => _options.ReportHistogramBuckets;
    public int MaxExportRows => _options.MaxExportRows;
    public int MaxViolationsCap => _options.MaxViolationsCap;

    public Task<int> GetMaxAttemptsPerAssignmentAsync(
        CancellationToken cancellationToken = default
    ) =>
        _settings.GetIntAsync(
            SystemSettingKeys.MaxAttemptsPerAssignment,
            _options.MaxAttemptsCap,
            cancellationToken
        );
}

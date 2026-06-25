using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Exams;

// Per-teacher exam cap: system_settings (live) with the appsettings value as fallback (MVP-7).
public sealed class ExamPolicy : IExamPolicy
{
    private readonly ExamOptions _options;
    private readonly ISystemSettingsService _settings;

    public ExamPolicy(IOptions<ExamOptions> options, ISystemSettingsService settings)
    {
        _options = options.Value;
        _settings = settings;
    }

    public Task<int> GetMaxExamsPerTeacherAsync(CancellationToken cancellationToken = default) =>
        _settings.GetIntAsync(
            SystemSettingKeys.MaxExamsPerTeacher,
            _options.MaxExamsPerTeacher,
            cancellationToken
        );
}

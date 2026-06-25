using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Questions;

// Per-teacher question cap: system_settings (live) with the appsettings value as fallback (MVP-7).
public sealed class QuestionPolicy : IQuestionPolicy
{
    private readonly QuestionBankOptions _options;
    private readonly ISystemSettingsService _settings;

    public QuestionPolicy(IOptions<QuestionBankOptions> options, ISystemSettingsService settings)
    {
        _options = options.Value;
        _settings = settings;
    }

    public Task<int> GetMaxQuestionsPerTeacherAsync(
        CancellationToken cancellationToken = default
    ) =>
        _settings.GetIntAsync(
            SystemSettingKeys.MaxQuestionsPerTeacher,
            _options.MaxQuestionsPerTeacher,
            cancellationToken
        );
}

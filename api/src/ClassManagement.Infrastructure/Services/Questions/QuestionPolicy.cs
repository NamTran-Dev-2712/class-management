using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Questions;

// Exposes question-bank tunables (bound from appsettings) to Application handlers via IQuestionPolicy.
public sealed class QuestionPolicy : IQuestionPolicy
{
    private readonly QuestionBankOptions _options;

    public QuestionPolicy(IOptions<QuestionBankOptions> options)
    {
        _options = options.Value;
    }

    public int MaxQuestionsPerTeacher => _options.MaxQuestionsPerTeacher;
}

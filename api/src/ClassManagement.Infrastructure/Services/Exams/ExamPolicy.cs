using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Exams;

// Exposes exam-builder tunables (bound from appsettings) to Application handlers via IExamPolicy.
public sealed class ExamPolicy : IExamPolicy
{
    private readonly ExamOptions _options;

    public ExamPolicy(IOptions<ExamOptions> options)
    {
        _options = options.Value;
    }

    public int MaxExamsPerTeacher => _options.MaxExamsPerTeacher;
}

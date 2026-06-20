using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Assignments;

// Exposes assignment tunables (bound from appsettings) to Application handlers via IAssignmentPolicy.
public sealed class AssignmentPolicy : IAssignmentPolicy
{
    private readonly AssignmentOptions _options;

    public AssignmentPolicy(IOptions<AssignmentOptions> options)
    {
        _options = options.Value;
    }

    public int MaxAssignmentsPerTeacher => _options.MaxAssignmentsPerTeacher;
    public int MaxTimeLimitMinutes => _options.MaxTimeLimitMinutes;
    public int MaxAttemptsCap => _options.MaxAttemptsCap;
    public int LifecycleBatchSize => _options.LifecycleBatchSize;
    public int ReportHistogramBuckets => _options.ReportHistogramBuckets;
    public int MaxExportRows => _options.MaxExportRows;
}

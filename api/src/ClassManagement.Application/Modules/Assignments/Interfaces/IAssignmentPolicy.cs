// Application-facing view of assignment tunables. Implemented in Infrastructure over
// IOptions<AssignmentOptions> so handlers stay free of the Options/config dependency.
public interface IAssignmentPolicy
{
    /// <summary>Max assignments a teacher may own; <c>0</c> = unlimited.</summary>
    int MaxAssignmentsPerTeacher { get; }

    /// <summary>Upper bound for an assignment's time limit, in minutes.</summary>
    int MaxTimeLimitMinutes { get; }

    /// <summary>Absolute upper bound a teacher may configure (appsettings; used by the write validator).</summary>
    int MaxAttemptsCap { get; }

    /// <summary>Live global ceiling on attempts per assignment (system_settings); <c>0</c> = unlimited.</summary>
    Task<int> GetMaxAttemptsPerAssignmentAsync(CancellationToken cancellationToken = default);

    /// <summary>Max expired attempts auto-submitted per lifecycle sweep.</summary>
    int LifecycleBatchSize { get; }

    /// <summary>Number of equal-width buckets in the assignment report's score histogram.</summary>
    int ReportHistogramBuckets { get; }

    /// <summary>Soft cap on rows in a CSV grade export.</summary>
    int MaxExportRows { get; }

    /// <summary>Absolute upper bound for an assignment's MaxViolations (MVP-10, write validator).</summary>
    int MaxViolationsCap { get; }
}

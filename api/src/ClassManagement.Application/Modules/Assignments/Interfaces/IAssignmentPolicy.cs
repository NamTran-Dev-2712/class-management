// Application-facing view of assignment tunables. Implemented in Infrastructure over
// IOptions<AssignmentOptions> so handlers stay free of the Options/config dependency.
public interface IAssignmentPolicy
{
    /// <summary>Max assignments a teacher may own; <c>0</c> = unlimited.</summary>
    int MaxAssignmentsPerTeacher { get; }

    /// <summary>Upper bound for an assignment's time limit, in minutes.</summary>
    int MaxTimeLimitMinutes { get; }

    /// <summary>Upper bound on attempts allowed per assignment.</summary>
    int MaxAttemptsCap { get; }

    /// <summary>Max expired attempts auto-submitted per lifecycle sweep.</summary>
    int LifecycleBatchSize { get; }
}

namespace ClassManagement.Infrastructure.Configuration;

public sealed class AssignmentOptions
{
    public const string SectionName = "Assignment";

    /// <summary>
    /// Max number of assignments a teacher may own (excluding soft-deleted). <c>0</c> = unlimited.
    /// </summary>
    public int MaxAssignmentsPerTeacher { get; init; } = 0;

    /// <summary>
    /// Minimum seconds between client auto-saves of an attempt; also the basis for the auto-save rate
    /// limit (MVP-5 §12 "auto-save overload" mitigation). The client debounces to this interval.
    /// </summary>
    public int AutoSaveMinIntervalSeconds { get; init; } = 30;

    /// <summary>
    /// Cron expression for the recurring lifecycle sweep job (Scheduled→Open, Open→Closed, auto-submit
    /// expired attempts). Default: every minute.
    /// </summary>
    public string LifecycleSweepCron { get; init; } = "* * * * *";

    /// <summary>Upper bound for an assignment's time limit, in minutes (24h). See BR-5-04.</summary>
    public int MaxTimeLimitMinutes { get; init; } = 1440;

    /// <summary>Upper bound on attempts allowed per assignment.</summary>
    public int MaxAttemptsCap { get; init; } = 100;

    /// <summary>Max expired attempts auto-submitted per lifecycle sweep (keeps one run bounded).</summary>
    public int LifecycleBatchSize { get; init; } = 200;
}

namespace ClassManagement.Domain.Modules.Assignments.Constants;

/// <summary>
/// Canonical <c>attempt_events.event_type</c> values (MVP-10). PascalCase, kept in sync with the CHECK
/// constraint in AttemptEventConfiguration and docs/database/06-schema-attempt.md. Never inline a literal.
/// The client emits these signals; the server records them and counts each one toward
/// <c>attempts.violation_count</c> (server is authoritative — BR-10-03).
/// </summary>
public static class AttemptEventTypes
{
    public const string TabSwitch = "TabSwitch";
    public const string FocusLoss = "FocusLoss";
    public const string FullscreenExit = "FullscreenExit";
    public const string CopyAttempt = "CopyAttempt";
    public const string PasteAttempt = "PasteAttempt";
    public const string ContextMenu = "ContextMenu";

    // Best-effort, low-confidence signal — log but do not use as sole disciplinary basis (BR-10-09).
    public const string DevToolsOpen = "DevToolsOpen";
    public const string ReloadAttempt = "ReloadAttempt";
    public const string InactivityTimeout = "InactivityTimeout";

    /// <summary>All valid event-type values — drives the CHECK constraint + validation.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        TabSwitch,
        FocusLoss,
        FullscreenExit,
        CopyAttempt,
        PasteAttempt,
        ContextMenu,
        DevToolsOpen,
        ReloadAttempt,
        InactivityTimeout,
    };
}

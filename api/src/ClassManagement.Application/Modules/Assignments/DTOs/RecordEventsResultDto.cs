namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// Server response after recording a batch of proctoring events (MVP-10). The client reacts to
/// <see cref="Submitted"/> (threshold auto-submit) / <see cref="Locked"/> (attempt locked) and shows the
/// authoritative <see cref="ViolationCount"/> against <see cref="MaxViolations"/>.
/// </summary>
public sealed record RecordEventsResultDto
{
    public int ViolationCount { get; init; }
    public int MaxViolations { get; init; }

    /// <summary>The action applied when the threshold was exceeded (WarnOnly/AutoSubmit/LockAttempt).</summary>
    public string Action { get; init; } = string.Empty;
    public bool ThresholdExceeded { get; init; }
    public bool Submitted { get; init; }
    public bool Locked { get; init; }
}

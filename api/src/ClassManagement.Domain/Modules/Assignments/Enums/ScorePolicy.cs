namespace ClassManagement.Domain.Modules.Assignments.Enums;

/// <summary>
/// When an assignment allows multiple attempts, which attempt's score counts (BR not numbered;
/// MVP-5 §3). <c>Highest</c> keeps the best, <c>Latest</c> keeps the most recent.
/// </summary>
public enum ScorePolicy
{
    Highest,
    Latest,
}

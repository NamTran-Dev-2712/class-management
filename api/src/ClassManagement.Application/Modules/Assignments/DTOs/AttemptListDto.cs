namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>Row for teacher submission rosters and student attempt history. Read from <c>vw_attempts</c>.</summary>
public sealed record AttemptListDto
{
    public Guid PublicId { get; init; }
    public string Status { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? DeadlineAt { get; init; }
    public bool AutoSubmitted { get; init; }

    public decimal? TotalAutoScore { get; init; }
    public decimal? TotalManualScore { get; init; }
    public decimal? TotalScore { get; init; }
    public decimal? TotalPoint { get; init; }

    public Guid AssignmentPublicId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public Guid StudentPublicId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

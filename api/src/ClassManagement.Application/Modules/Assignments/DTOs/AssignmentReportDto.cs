namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// Aggregate report for one assignment (MVP-6, T6-08): submission counts, score statistics, a score
/// histogram, and the per-student grade table. The effective per-student score honours the assignment's
/// <c>score_policy</c> (Highest = max; Latest = the latest graded attempt). Teacher/admin only.
/// </summary>
public sealed record AssignmentReportDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ScorePolicy { get; init; } = string.Empty;
    public string GradePublishPolicy { get; init; } = string.Empty;
    public DateTime? GradesReleasedAt { get; init; }
    public decimal? TotalPoint { get; init; }

    public int TotalStudents { get; init; }
    public int SubmittedCount { get; init; }
    public int NotSubmittedCount { get; init; }
    public int GradedCount { get; init; }
    public int PendingGradingCount { get; init; }

    public decimal? AverageScore { get; init; }
    public decimal? MinScore { get; init; }
    public decimal? MaxScore { get; init; }

    public IReadOnlyList<ReportBucketDto> Buckets { get; init; } = [];
    public IReadOnlyList<ReportStudentRowDto> Students { get; init; } = [];
}

/// <summary>A score-distribution histogram bucket: [RangeStart, RangeEnd] with the student count.</summary>
public sealed record ReportBucketDto(decimal RangeStart, decimal RangeEnd, int Count, string Label);

/// <summary>One student's row in the grade table.</summary>
public sealed record ReportStudentRowDto
{
    public Guid StudentPublicId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public bool Submitted { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal? EffectiveScore { get; init; }
    public int AttemptCount { get; init; }
}

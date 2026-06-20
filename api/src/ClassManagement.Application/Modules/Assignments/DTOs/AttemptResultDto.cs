namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// An attempt's result. Score visibility honours the assignment's grade-publish policy: when grades
/// aren't released yet, <see cref="ScoreReleased"/> is false and the score fields are null (the FE
/// shows "pending"). Per-question breakdown — including teacher feedback and (when
/// <see cref="ShowAnswers"/>) the correct options — is included only when released.
/// </summary>
public sealed record AttemptResultDto
{
    public Guid PublicId { get; init; }
    public Guid AssignmentPublicId { get; init; }
    public string AssignmentTitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public bool AutoSubmitted { get; init; }

    public decimal? TotalPoint { get; init; }
    public bool ScoreReleased { get; init; }

    /// <summary>True when the student may see correct answers (show_answers_after_grade + released).</summary>
    public bool ShowAnswers { get; init; }
    public decimal? TotalAutoScore { get; init; }
    public decimal? TotalManualScore { get; init; }
    public decimal? TotalScore { get; init; }

    public IReadOnlyList<AttemptAnswerResultDto> Answers { get; init; } = [];
}

/// <summary>Per-question result (populated only when the score is released).</summary>
public sealed record AttemptAnswerResultDto
{
    public Guid QuestionPublicId { get; init; }
    public int DisplayPosition { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal Point { get; init; }
    public bool IsWriting { get; init; }

    public decimal? AutoScore { get; init; }
    public bool IsAutoGraded { get; init; }

    // Writing
    public string? TextAnswer { get; init; }
    public decimal? ManualScore { get; init; }
    public string? Feedback { get; init; }

    // Objective options. <c>IsCorrect</c> is null unless the teacher allows revealing answers.
    public IReadOnlyList<AttemptAnswerOptionDto> Options { get; init; } = [];
}

/// <summary>A snapshot option in the student's result: what they selected and (optionally) the key.</summary>
public sealed record AttemptAnswerOptionDto
{
    public Guid PublicId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsSelected { get; init; }
    public bool? IsCorrect { get; init; }
}

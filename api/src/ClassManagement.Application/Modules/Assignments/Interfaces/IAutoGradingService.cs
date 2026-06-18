using ClassManagement.Domain.Modules.Questions.Enums;

namespace ClassManagement.Application.Modules.Assignments.Interfaces;

/// <summary>
/// Pure auto-grading over snapshot data (no DB). Objective questions (SingleChoice/TrueFalse exact
/// match; MultipleChoice all-or-nothing per BR-5-12) get a score; writing questions return a null
/// score and flag manual grading (MVP-6). Stateless and unit-testable.
/// </summary>
public interface IAutoGradingService
{
    AttemptGradingResult Grade(IReadOnlyList<GradingQuestion> questions);
}

/// <summary>One question's grading inputs, drawn from the attempt's snapshot + the student's answer.</summary>
public sealed record GradingQuestion(
    long SnapshotQuestionId,
    QuestionType Type,
    decimal Point,
    IReadOnlyList<long> CorrectOptionIds,
    IReadOnlyList<long>? SelectedOptionIds,
    bool HasTextAnswer
);

/// <summary>Per-answer grading output. <c>AutoScore</c> is null for writing questions (pending manual).</summary>
public sealed record GradedAnswer(long SnapshotQuestionId, decimal? AutoScore, bool IsAutoGraded);

/// <summary>Whole-attempt grading output.</summary>
public sealed record AttemptGradingResult(
    IReadOnlyList<GradedAnswer> Answers,
    decimal TotalAutoScore,
    bool HasManualPending
);

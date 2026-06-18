using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Questions.Enums;

namespace ClassManagement.Infrastructure.Services.Assignments;

// Pure auto-grading over snapshot data (BR-5-12). SingleChoice/TrueFalse: full point on an exact
// single-option match. MultipleChoice: all-or-nothing — full point only when the selected set equals
// the correct set (no partial score in MVP-5). Writing questions get no auto score and flag manual
// grading (MVP-6). Stateless.
public sealed class AutoGradingService : IAutoGradingService
{
    public AttemptGradingResult Grade(IReadOnlyList<GradingQuestion> questions)
    {
        var graded = new List<GradedAnswer>(questions.Count);
        decimal totalAutoScore = 0m;
        var hasManualPending = false;

        foreach (var q in questions)
        {
            switch (q.Type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                case QuestionType.MultipleChoice:
                    var score = IsCorrect(q.CorrectOptionIds, q.SelectedOptionIds) ? q.Point : 0m;
                    totalAutoScore += score;
                    graded.Add(new GradedAnswer(q.SnapshotQuestionId, score, true));
                    break;

                case QuestionType.ShortWriting:
                case QuestionType.LongWriting:
                default:
                    hasManualPending = true;
                    graded.Add(new GradedAnswer(q.SnapshotQuestionId, null, false));
                    break;
            }
        }

        return new AttemptGradingResult(graded, totalAutoScore, hasManualPending);
    }

    // Exact set match: every correct option selected, no extra option selected (all-or-nothing).
    private static bool IsCorrect(IReadOnlyList<long> correct, IReadOnlyList<long>? selected)
    {
        if (correct.Count == 0)
            return false;

        var selectedSet = selected is null ? [] : new HashSet<long>(selected);
        var correctSet = new HashSet<long>(correct);
        return selectedSet.SetEquals(correctSet);
    }
}

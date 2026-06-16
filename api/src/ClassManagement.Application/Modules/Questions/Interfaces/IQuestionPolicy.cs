// Application-facing view of question-bank tunables. Implemented in Infrastructure over
// IOptions<QuestionBankOptions> so handlers stay free of the Options/config dependency.
public interface IQuestionPolicy
{
    /// <summary>Max questions a teacher may own; <c>0</c> = unlimited.</summary>
    int MaxQuestionsPerTeacher { get; }
}

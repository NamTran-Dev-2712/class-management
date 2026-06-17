// Application-facing view of exam-builder tunables. Implemented in Infrastructure over
// IOptions<ExamOptions> so handlers stay free of the Options/config dependency.
public interface IExamPolicy
{
    /// <summary>Max exams a teacher may own; <c>0</c> = unlimited.</summary>
    int MaxExamsPerTeacher { get; }
}

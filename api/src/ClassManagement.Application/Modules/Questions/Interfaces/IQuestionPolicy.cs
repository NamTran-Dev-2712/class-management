// Application-facing view of question-bank tunables. The per-teacher cap is read (cached) from
// system_settings with the appsettings value as fallback so an admin can change it live (MVP-7).
public interface IQuestionPolicy
{
    /// <summary>Max questions a teacher may own; <c>0</c> = unlimited.</summary>
    Task<int> GetMaxQuestionsPerTeacherAsync(CancellationToken cancellationToken = default);
}

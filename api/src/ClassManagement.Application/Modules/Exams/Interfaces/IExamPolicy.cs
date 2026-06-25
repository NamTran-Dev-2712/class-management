// Application-facing view of exam-builder tunables. The per-teacher cap is read (cached) from
// system_settings with the appsettings value as fallback so an admin can change it live (MVP-7).
public interface IExamPolicy
{
    /// <summary>Max exams a teacher may own; <c>0</c> = unlimited.</summary>
    Task<int> GetMaxExamsPerTeacherAsync(CancellationToken cancellationToken = default);
}

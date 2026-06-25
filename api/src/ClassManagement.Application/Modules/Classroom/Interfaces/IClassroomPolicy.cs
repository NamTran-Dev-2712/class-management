// Application-facing view of classroom tunables. Implemented in Infrastructure: the per-teacher cap is
// read (cached) from system_settings with the appsettings value as fallback, so an admin can change it
// live (MVP-7); other config stays on Options.
public interface IClassroomPolicy
{
    /// <summary>Max classes a teacher may own; <c>0</c> = unlimited.</summary>
    Task<int> GetMaxClassesPerTeacherAsync(CancellationToken cancellationToken = default);

    /// <summary>Max approved students per class; <c>0</c> = unlimited.</summary>
    Task<int> GetMaxStudentsPerClassAsync(CancellationToken cancellationToken = default);

    /// <summary>Length of generated invite codes (clamped to 6–12).</summary>
    Task<int> GetInviteCodeLengthAsync(CancellationToken cancellationToken = default);
}

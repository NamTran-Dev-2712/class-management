// Application-facing view of classroom tunables. Implemented in Infrastructure over
// IOptions<ClassroomOptions> so handlers stay free of the Options/config dependency.
public interface IClassroomPolicy
{
    /// <summary>Max classes a teacher may own; <c>0</c> = unlimited.</summary>
    int MaxClassesPerTeacher { get; }
}

namespace ClassManagement.Application.Common.Constants;

public static class CacheKeys
{
    public static string UserProfile(long userId) => $"user:profile:{userId}";

    public static string UserRoles(long userId) => $"user:roles:{userId}";

    public static string SubjectList() => "catalog:subjects:active";

    public static string SubjectDetail(Guid publicId) => $"catalog:subjects:{publicId}";
}

/// <summary>
/// Named output-cache policy ids (one per read surface). Applied on GET actions via
/// <c>[OutputCache(PolicyName = …)]</c>; registered in <c>Api/DependencyInjection</c>.
/// </summary>
public static class OutputCachePolicies
{
    public const string SubjectsRead = "subjects-read";
    public const string UsersRead = "users-read";
    public const string AdminClassesRead = "admin-classes-read";
    public const string TeacherClassesRead = "teacher-classes-read";
    public const string StudentClassesRead = "student-classes-read";
    public const string TeacherQuestionsRead = "teacher-questions-read";
    public const string PublicQuestionsRead = "public-questions-read";
    public const string AdminQuestionsRead = "admin-questions-read";
    public const string TeacherExamsRead = "teacher-exams-read";
    public const string PublicExamsRead = "public-exams-read";
    public const string AdminExamsRead = "admin-exams-read";
}

/// <summary>
/// Output-cache tags (one per resource domain). Read policies tag their entries; write controllers
/// evict the matching tag via <c>IOutputCacheStore.EvictByTagAsync</c> on a successful mutation.
/// </summary>
public static class OutputCacheTags
{
    public const string Subjects = "subjects";
    public const string Users = "users";
    public const string Classrooms = "classrooms";
    public const string Questions = "questions";
    public const string Exams = "exams";
}

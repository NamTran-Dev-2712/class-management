namespace ClassManagement.Application.Common.Constants;

public static class CacheKeys
{
    public static string UserProfile(long userId) => $"user:profile:{userId}";

    public static string UserRoles(long userId) => $"user:roles:{userId}";

    public static string SubjectList() => "catalog:subjects:active";

    public static string SubjectDetail(Guid publicId) => $"catalog:subjects:{publicId}";
}

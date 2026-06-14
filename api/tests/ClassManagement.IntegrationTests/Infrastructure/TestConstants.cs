namespace ClassManagement.IntegrationTests.Infrastructure;

public static class TestConstants
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin@123456";

    public const string DefaultPassword = "Test@123456";
    public const string DefaultDisplayName = "Test User";

    public static string UniqueEmail() => $"user_{Guid.NewGuid():N}@test.local";
}

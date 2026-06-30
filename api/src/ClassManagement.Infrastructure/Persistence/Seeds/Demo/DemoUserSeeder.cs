using ClassManagement.Application.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Seeds deterministic demo teachers + students via UserManager (so password hashing + role assignment
// go through Identity). Shared password Demo@123456 for every demo account.
internal static class DemoUserSeeder
{
    public const string Password = "Demo@123456";

    private static readonly (string Email, string Name)[] TeacherAccounts =
    [
        ("teacher1@demo.local", "Nguyễn Văn An"),
        ("teacher2@demo.local", "Trần Thị Bình"),
        ("teacher3@demo.local", "Lê Hoàng Cường"),
    ];

    private static readonly string[] StudentNames =
    [
        "Phạm Minh Anh",
        "Vũ Thị Hương",
        "Đỗ Quang Huy",
        "Bùi Thảo My",
        "Hoàng Đức Duy",
        "Ngô Khánh Linh",
        "Đặng Gia Bảo",
        "Lý Phương Thảo",
        "Trịnh Tuấn Kiệt",
        "Mai Ngọc Hân",
    ];

    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        DemoSeedState state,
        ILogger logger
    )
    {
        foreach (var (email, name) in TeacherAccounts)
        {
            var user = await CreateAsync(
                userManager,
                email,
                name,
                ApplicationRoles.Teacher,
                logger
            );
            if (user is not null)
                state.Teachers.Add(user);
        }

        for (var i = 1; i <= 25; i++)
        {
            var email = $"student{i}@demo.local";
            var name = $"{StudentNames[(i - 1) % StudentNames.Length]} {i:D2}";
            var user = await CreateAsync(
                userManager,
                email,
                name,
                ApplicationRoles.Student,
                logger
            );
            if (user is not null)
                state.Students.Add(user);
        }

        logger.LogInformation(
            "Seeded {TeacherCount} demo teachers + {StudentCount} demo students",
            state.Teachers.Count,
            state.Students.Count
        );
    }

    private static async Task<ApplicationUser?> CreateAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role,
        ILogger logger
    )
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to create demo user {Email}: {Errors}",
                email,
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}

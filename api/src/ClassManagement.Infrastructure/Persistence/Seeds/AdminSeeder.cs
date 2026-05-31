using ClassManagement.Application.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger
    )
    {
        var email = configuration["Seed:AdminEmail"] ?? "admin@classmanagement.local";
        var password = configuration["Seed:AdminPassword"] ?? "Admin@123456";
        var displayName = configuration["Seed:AdminDisplayName"] ?? "System Administrator";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to create admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
            return;
        }

        await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
        logger.LogInformation("Seeded admin user: {Email}", email);
    }
}

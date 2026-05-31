using ClassManagement.Application.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        foreach (var roleName in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = $"{roleName} role",
                CreatedAt = DateTime.UtcNow,
            };

            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
                logger.LogInformation("Seeded role: {Role}", roleName);
            else
                logger.LogError(
                    "Failed to seed role {Role}: {Errors}",
                    roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
        }
    }
}

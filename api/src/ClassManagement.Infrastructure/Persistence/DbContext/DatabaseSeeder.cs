using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Infrastructure.Persistence.Seeds;
using ClassManagement.Infrastructure.Persistence.Seeds.Demo;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.DbContext;

public static class DatabaseSeeder
{
    // Unique lock key — avoids collisions with other apps on the same Postgres instance
    private const long SeedLockKey = 0x436C61_73734D67L; // "ClassMg" as hex

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();

            // Serialize concurrent seeders (e.g. dotnet watch double-start) via advisory lock
            await context.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_lock({SeedLockKey});");

            try
            {
                await RunSeedAsync(context, sp, logger);
            }
            finally
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"SELECT pg_advisory_unlock({SeedLockKey});"
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding");
            throw;
        }
    }

    private static async Task RunSeedAsync(
        ApplicationDbContext context,
        IServiceProvider sp,
        ILogger logger
    )
    {
        // Step 1: citext extension — no table dependency, must run before schema creation
        await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS citext;");

        // Step 2: Ensure schema exists.
        //   • If migration files exist  → MigrateAsync applies pending ones.
        //   • If no migration files yet → EnsureCreatedAsync creates schema directly
        //     from the EF model (good for dev; generate migrations later for prod).
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        var appliedMigrations = (await context.Database.GetAppliedMigrationsAsync()).ToList();

        if (pendingMigrations.Count > 0)
        {
            logger.LogInformation(
                "Applying {Count} pending migration(s)...",
                pendingMigrations.Count
            );
            await context.Database.MigrateAsync();
        }
        else if (appliedMigrations.Count == 0)
        {
            // No migrations generated yet — create schema directly from EF model
            logger.LogWarning(
                "No EF migrations found. Creating schema via EnsureCreated (dev only). "
                    + "Run 'dotnet ef migrations add InitDatabase' to switch to migration-based flow."
            );
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            logger.LogInformation(
                "All {Count} migration(s) already applied.",
                appliedMigrations.Count
            );
        }

        // Step 3: DB triggers — AFTER tables exist (idempotent, safe on every startup)
        await ApplyTriggersAsync(context);

        // Step 4: Seed reference data
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        await RoleSeeder.SeedAsync(roleManager, logger);

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = sp.GetRequiredService<IConfiguration>();
        await AdminSeeder.SeedAsync(userManager, configuration, logger);

        await SubjectSeeder.SeedAsync(context, logger);
        await SystemSettingSeeder.SeedAsync(context, logger);
        await PlanSeeder.SeedAsync(context, logger);

        // Step 5: Dev-only demo data — gated by environment (hard production backstop) AND the
        // Seed:DemoData flag (default false; on in appsettings.Development.json; forced off in tests).
        var environment = sp.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment() && configuration.GetValue<bool>("Seed:DemoData"))
        {
            var autoGrader = sp.GetRequiredService<IAutoGradingService>();
            await DemoDataSeeder.SeedAsync(context, userManager, autoGrader, logger);
        }
    }

    // set_updated_at() function + per-table triggers — idempotent (DROP IF EXISTS + CREATE)
    private static Task ApplyTriggersAsync(ApplicationDbContext context) =>
        context.Database.ExecuteSqlRawAsync(
            """
            CREATE OR REPLACE FUNCTION set_updated_at()
            RETURNS TRIGGER AS $$
            BEGIN
              NEW.updated_at = NOW();
              RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_users_updated_at ON users;
            CREATE TRIGGER trg_users_updated_at
              BEFORE UPDATE ON users
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_subjects_updated_at ON subjects;
            CREATE TRIGGER trg_subjects_updated_at
              BEFORE UPDATE ON subjects
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_classes_updated_at ON classes;
            CREATE TRIGGER trg_classes_updated_at
              BEFORE UPDATE ON classes
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_reports_updated_at ON reports;
            CREATE TRIGGER trg_reports_updated_at
              BEFORE UPDATE ON reports
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_system_settings_updated_at ON system_settings;
            CREATE TRIGGER trg_system_settings_updated_at
              BEFORE UPDATE ON system_settings
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_plans_updated_at ON plans;
            CREATE TRIGGER trg_plans_updated_at
              BEFORE UPDATE ON plans
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_subscriptions_updated_at ON subscriptions;
            CREATE TRIGGER trg_subscriptions_updated_at
              BEFORE UPDATE ON subscriptions
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();

            DROP TRIGGER IF EXISTS trg_media_assets_updated_at ON media_assets;
            CREATE TRIGGER trg_media_assets_updated_at
              BEFORE UPDATE ON media_assets
              FOR EACH ROW EXECUTE FUNCTION set_updated_at();
            """
        );
}

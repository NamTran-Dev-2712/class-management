using ClassManagement.Infrastructure.Persistence;
using ClassManagement.Infrastructure.Persistence.Cache;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Persistence.Interceptors;
using ClassManagement.Infrastructure.Security;
using ClassManagement.Infrastructure.Services.Cache;
using ClassManagement.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Interceptor registered as singleton (ICurrentUserService resolved per-scope inside it)
        services.AddSingleton<AuditableEntityInterceptor>();

        // DbContext pooling — UseSnakeCaseNamingConvention maps all C# PascalCase to snake_case
        services.AddDbContextPool<ApplicationDbContext>(
            (sp, opts) =>
            {
                opts.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        npgsql => npgsql.EnableRetryOnFailure(3).CommandTimeout(30)
                    )
                    .UseSnakeCaseNamingConvention();

                opts.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());

#if DEBUG
                opts.EnableSensitiveDataLogging().EnableDetailedErrors();
#endif
            }
        );

        // ASP.NET Core Identity
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(opts =>
            {
                opts.Password.RequiredLength = 8;
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.User.RequireUniqueEmail = true;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Redis cache
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = configuration["Redis:ConnectionString"];
            opts.InstanceName = "class_mgmt:";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // JWT options (skeleton — full wiring added when auth module is implemented)
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Generic repository + Unit of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Current user from HTTP context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

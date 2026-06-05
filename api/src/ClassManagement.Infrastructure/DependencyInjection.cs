using System.Text;
using ClassManagement.Infrastructure.Persistence;
using ClassManagement.Infrastructure.Persistence.Cache;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Persistence.Interceptors;
using ClassManagement.Infrastructure.Security;
using ClassManagement.Infrastructure.Services.Cache;
using ClassManagement.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        // JWT Bearer authentication
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services
            .AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        // Bind JwtBearerOptions lazily from IOptions<JwtOptions> so the validation key is the
        // exact same one JwtTokenService signs with (single source of truth, no eager capture).
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (bearer, jwt) =>
                {
                    var o = jwt.Value;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = o.Issuer,
                        ValidAudience = o.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(o.SecretKey)
                        ),
                        ClockSkew = TimeSpan.Zero,
                    };

                    // Read access token from cookie when Authorization header is absent
                    bearer.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            if (string.IsNullOrEmpty(ctx.Token))
                                ctx.Token = ctx.Request.Cookies["access_token"];
                            return Task.CompletedTask;
                        },
                    };
                }
            );
        services.AddAuthorization();

        // Generic repository + Unit of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth services
        services.AddScoped<ITokenHasher, TokenHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();

        // Current user from HTTP context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

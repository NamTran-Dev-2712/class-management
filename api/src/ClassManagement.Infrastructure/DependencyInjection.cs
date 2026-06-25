using System.Text;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Persistence;
using ClassManagement.Infrastructure.Persistence.Cache;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Persistence.Interceptors;
using ClassManagement.Infrastructure.Security;
using ClassManagement.Infrastructure.Services.Cache;
using ClassManagement.Infrastructure.Services.Email;
using ClassManagement.Infrastructure.Services.Identity;
using ClassManagement.Infrastructure.Services.Localization;
using ClassManagement.Infrastructure.Services.Messaging;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Resend;

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

                    // Read the access token from the cookie when the Authorization header is absent; for
                    // SignalR WebSocket upgrades (where custom headers aren't possible) also accept it from
                    // the `access_token` query string on /hubs paths.
                    bearer.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            if (string.IsNullOrEmpty(ctx.Token))
                            {
                                var path = ctx.HttpContext.Request.Path;
                                if (path.StartsWithSegments("/hubs"))
                                {
                                    var queryToken = ctx.Request.Query["access_token"];
                                    if (!string.IsNullOrEmpty(queryToken))
                                        ctx.Token = queryToken;
                                }

                                if (string.IsNullOrEmpty(ctx.Token))
                                    ctx.Token = ctx.Request.Cookies["access_token"];
                            }

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
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();

        // Catalog
        services.AddScoped<ISubjectRepository, SubjectRepository>();

        // Classroom (MVP-2)
        services.Configure<ClassroomOptions>(
            configuration.GetSection(ClassroomOptions.SectionName)
        );
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassMembershipRepository, ClassMembershipRepository>();
        services.AddSingleton<IInviteCodeGenerator, Services.Classroom.InviteCodeGenerator>();
        // Scoped (not Singleton): the policy now reads the live, cached system_settings value.
        services.AddScoped<IClassroomPolicy, Services.Classroom.ClassroomPolicy>();

        // Question bank (MVP-3)
        services.Configure<QuestionBankOptions>(
            configuration.GetSection(QuestionBankOptions.SectionName)
        );
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuestionPolicy, Services.Questions.QuestionPolicy>();

        // Exam builder (MVP-4)
        services.Configure<ExamOptions>(configuration.GetSection(ExamOptions.SectionName));
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamPolicy, Services.Exams.ExamPolicy>();

        // Assignment & online testing (MVP-5)
        services.Configure<AssignmentOptions>(
            configuration.GetSection(AssignmentOptions.SectionName)
        );
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();
        services.AddScoped<IManualGradeRepository, ManualGradeRepository>();
        // Scoped (not Singleton): the policy now reads the live, cached system_settings value.
        services.AddScoped<IAssignmentPolicy, Services.Assignments.AssignmentPolicy>();
        services.AddSingleton<
            Application.Modules.Assignments.Interfaces.IAutoGradingService,
            Services.Assignments.AutoGradingService
        >();
        services.AddScoped<
            Application.Modules.Assignments.Interfaces.IGradeExportService,
            Services.Assignments.CsvGradeExportService
        >();

        // Admin user management
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();

        // Admin & moderation (MVP-7)
        services.AddScoped<IAuditLogger, Services.Audit.AuditLogger>();
        services.AddScoped<INotificationService, Services.Notifications.NotificationService>();
        services.AddScoped<ISystemSettingsService, Services.Settings.SystemSettingsService>();
        services.Configure<NotificationOptions>(
            configuration.GetSection(NotificationOptions.SectionName)
        );
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        // Password-reset / email / client-app options
        services.Configure<PasswordResetOptions>(
            configuration.GetSection(PasswordResetOptions.SectionName)
        );
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
        services.Configure<ClientAppOptions>(
            configuration.GetSection(ClientAppOptions.SectionName)
        );

        // Email transport (Resend) + background email queue
        services.Configure<ResendClientOptions>(o =>
            o.ApiToken = configuration[$"{ResendOptions.SectionName}:ApiKey"] ?? string.Empty
        );
        services.AddHttpClient<ResendClient>();
        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddScoped<IEmailQueueService, HangfireEmailQueueService>();

        // Hangfire — durable background jobs on PostgreSQL (gated by config so tests can opt out)
        var hangfire =
            configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>()
            ?? new HangfireOptions();

        if (hangfire.Enabled)
        {
            services.AddHangfire(cfg =>
                cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(pg =>
                        pg.UseNpgsqlConnection(
                            configuration.GetConnectionString("DefaultConnection")
                        )
                    )
            );

            if (hangfire.EnableServer)
                services.AddHangfireServer(opts =>
                {
                    if (hangfire.WorkerCount > 0)
                        opts.WorkerCount = hangfire.WorkerCount;
                });
        }

        // Localization — JSON-backed message resolver driven by the request culture
        services.Configure<LocalizationOptions>(
            configuration.GetSection(LocalizationOptions.SectionName)
        );
        services.AddSingleton<ILocalizationService, JsonLocalizationService>();

        // Current user from HTTP context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserDirectory, UserDirectory>();

        return services;
    }
}

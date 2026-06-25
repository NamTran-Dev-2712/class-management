using System.Reflection;
using ClassManagement.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        // Per-request realtime outbox (buffer of users whose notifications changed).
        services.AddScoped<IRealtimeOutbox, RealtimeOutbox>();

        // Outermost behavior: flush the realtime outbox only after the whole pipeline + handler succeed.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RealtimeDispatchBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // Runs inner to validation: only valid, executed commands get coarse-audited.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            includeInternalTypes: true
        );

        return services;
    }
}

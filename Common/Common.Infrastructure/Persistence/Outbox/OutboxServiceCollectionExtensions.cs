using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Infrastructure.Persistence.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddCommonOutbox<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext
    {
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(
                options => options.IsValid(),
                "La sección 'Outbox' exige intervalos y tamaños positivos, y un tope de espera no menor que la base.")
            .ValidateOnStart();

        services.TryAddSingleton<OutboxInterceptor>();
        services.TryAddSingleton<OutboxMetrics>();
        services.AddHostedService<OutboxProcessor<TContext>>();

        services.AddHealthChecks().AddCheck<OutboxHealthCheck>(OutboxHealthCheck.Name, HealthStatus.Degraded);

        return services;
    }
}

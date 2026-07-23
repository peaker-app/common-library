using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            configure?.Invoke(bus);

            bus.UsingRabbitMq((context, rabbit) =>
            {
                MessagingOptions options = context.GetRequiredService<IOptions<MessagingOptions>>().Value;

                rabbit.Host(options.Host, options.Port, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);
                });

                rabbit.UseMessageRetry(retry => retry.Exponential(
                    5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

                rabbit.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

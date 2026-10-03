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
        EventBusRegistration registration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.EndpointPrefix);

        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));
        services.AddSingleton<MessagingMetrics>();
        services.AddSingleton<DeadLetterObserver>();

        services.AddMassTransit(bus =>
        {
            bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(registration.EndpointPrefix, false));
            registration.ConfigureConsumers?.Invoke(bus);

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

                rabbit.ConnectReceiveObserver(context.GetRequiredService<DeadLetterObserver>());

                rabbit.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static void AddTemporaryConsumer<TConsumer>(this IBusRegistrationConfigurator bus)
        where TConsumer : class, IConsumer =>
        bus.AddConsumer<TConsumer>().Endpoint(endpoint =>
        {
            endpoint.Temporary = true;
            endpoint.InstanceId = Environment.MachineName;
        });
}

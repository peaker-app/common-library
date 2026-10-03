using MassTransit;

namespace Common.Infrastructure.Messaging;

public sealed record EventBusRegistration(
    string EndpointPrefix,
    Action<IBusRegistrationConfigurator>? ConfigureConsumers = null);

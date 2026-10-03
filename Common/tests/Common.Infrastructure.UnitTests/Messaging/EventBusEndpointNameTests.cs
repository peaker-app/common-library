using Common.Infrastructure.Messaging;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common.Infrastructure.UnitTests.Messaging;

public sealed class EventBusEndpointNameTests
{
    [Fact]
    public void AddEventBus_PrefixesEveryConsumerQueueWithTheService()
    {
        using ServiceProvider provider = BuildProvider("account", bus => bus.AddConsumer<PeakRenamedConsumer>());

        provider.GetRequiredService<IEndpointNameFormatter>().Consumer<PeakRenamedConsumer>()
            .Should().Be("account-peak-renamed");
    }

    [Fact]
    public void AddTemporaryConsumer_NamesTheQueueAfterTheServiceAndTheInstance()
    {
        using ServiceProvider provider = BuildProvider("ascent", bus => bus.AddTemporaryConsumer<PeakUpdatedConsumer>());

        EndpointNameOf<PeakUpdatedConsumer>(provider)
            .Should().BeEquivalentTo($"ascent-peak-updated-{Environment.MachineName}");
    }

    [Fact]
    public void AddTemporaryConsumer_MarksTheEndpointAsTemporary()
    {
        using ServiceProvider provider = BuildProvider("ascent", bus => bus.AddTemporaryConsumer<PeakUpdatedConsumer>());

        DefinitionOf<PeakUpdatedConsumer>(provider).IsTemporary.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddEventBus_WithoutAPrefix_Throws(string prefix)
    {
        Action addEventBus = () => new ServiceCollection()
            .AddEventBus(new ConfigurationBuilder().Build(), new EventBusRegistration(prefix));

        addEventBus.Should().Throw<ArgumentException>();
    }

    private static ServiceProvider BuildProvider(string prefix, Action<IBusRegistrationConfigurator> consumers) =>
        new ServiceCollection()
            .AddEventBus(new ConfigurationBuilder().Build(), new EventBusRegistration(prefix, consumers))
            .BuildServiceProvider();

    private static string EndpointNameOf<TConsumer>(IServiceProvider provider)
        where TConsumer : class, IConsumer =>
        DefinitionOf<TConsumer>(provider).GetEndpointName(provider.GetRequiredService<IEndpointNameFormatter>());

    private static IEndpointDefinition DefinitionOf<TConsumer>(IServiceProvider provider)
        where TConsumer : class, IConsumer =>
        provider.GetRequiredService<IEndpointDefinition<TConsumer>>();

    private sealed class PeakRenamedConsumer : IConsumer<object>
    {
        public Task Consume(ConsumeContext<object> context) => Task.CompletedTask;
    }

    private sealed class PeakUpdatedConsumer : IConsumer<object>
    {
        public Task Consume(ConsumeContext<object> context) => Task.CompletedTask;
    }
}

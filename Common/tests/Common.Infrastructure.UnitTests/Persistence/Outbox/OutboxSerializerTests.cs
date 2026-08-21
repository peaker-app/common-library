using Common.Domain.Abstractions;
using Common.Infrastructure.Persistence.Outbox;
using FluentAssertions;
using Xunit;

namespace Common.Infrastructure.UnitTests.Persistence.Outbox;

public sealed class OutboxSerializerTests
{
    [Fact]
    public void Deserialize_WithAMessageWrittenBySerialize_RestoresTheEvent()
    {
        PeakNamed original = new(Guid.CreateVersion7(), "Mont Blanc");
        OutboxMessage message = MessageFor(original);

        IDomainEvent restored = OutboxSerializer.Deserialize(message);

        restored.Should().BeOfType<PeakNamed>().Which.Should().Be(original);
    }

    [Fact]
    public void Deserialize_WithAnUnresolvableType_Throws()
    {
        OutboxMessage message = new()
        {
            Id = Guid.CreateVersion7(),
            Type = "Peaker.Unknown.Event, Peaker.Unknown",
            Content = "{}",
            OccurredAtUtc = DateTime.UtcNow
        };

        FluentActions.Invoking(() => OutboxSerializer.Deserialize(message))
            .Should().Throw<InvalidOperationException>();
    }

    private static OutboxMessage MessageFor(IDomainEvent domainEvent) => new()
    {
        Id = Guid.CreateVersion7(),
        Type = domainEvent.GetType().AssemblyQualifiedName!,
        Content = OutboxSerializer.Serialize(domainEvent),
        OccurredAtUtc = DateTime.UtcNow
    };

    private sealed record PeakNamed(Guid PeakId, string Name) : IDomainEvent;
}

namespace Common.Contracts.Abstractions;

public abstract record IntegrationEvent
{
    public Guid MessageId { get; init; } = Guid.CreateVersion7();

    public Guid CorrelationId { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public int Version { get; init; } = 1;
}

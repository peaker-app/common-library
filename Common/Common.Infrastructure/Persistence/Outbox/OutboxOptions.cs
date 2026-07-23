namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(10);

    public int BatchSize { get; init; } = 20;
}

namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(10);

    public int BatchSize { get; init; } = 20;

    public int MaxAttempts { get; init; } = 12;

    public TimeSpan RetryBackoffBase { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan RetryBackoffCap { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan PendingAgeThreshold { get; init; } = TimeSpan.FromMinutes(5);

    public bool IsValid() =>
        PollingInterval > TimeSpan.Zero
        && BatchSize > 0
        && MaxAttempts > 0
        && RetryBackoffBase > TimeSpan.Zero
        && RetryBackoffCap >= RetryBackoffBase
        && PendingAgeThreshold > TimeSpan.Zero;
}

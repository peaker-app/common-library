namespace Common.Infrastructure.Persistence.Outbox;

public sealed record OutboxBacklog(int PendingCount, int ParkedCount, TimeSpan OldestPendingAge)
{
    public static OutboxBacklog Empty { get; } = new(0, 0, TimeSpan.Zero);

    public bool ExceedsThreshold(OutboxOptions options) =>
        ParkedCount > 0 || OldestPendingAge > options.PendingAgeThreshold;
}

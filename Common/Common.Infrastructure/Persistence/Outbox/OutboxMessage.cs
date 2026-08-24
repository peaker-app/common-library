namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Content { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public DateTime? ProcessedAtUtc { get; set; }

    public string? Error { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }
}

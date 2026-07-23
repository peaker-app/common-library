namespace Common.Infrastructure.Persistence.Idempotency;

public sealed class ProcessedMessage
{
    public required Guid MessageId { get; init; }

    public required DateTime ProcessedAtUtc { get; init; }
}

using System.Diagnostics.Metrics;
using Common.Infrastructure.Observability;

namespace Common.Infrastructure.Messaging;

public sealed class MessagingMetrics
{
    private readonly Counter<long> deadLettered;

    public MessagingMetrics() =>
        deadLettered = PeakerMetrics.Meter.CreateCounter<long>(
            "peaker.messaging.dead_lettered", "{message}");

    public void RecordDeadLettered(string queue, string errorType) =>
        deadLettered.Add(
            1,
            new KeyValuePair<string, object?>("messaging.destination", queue),
            new KeyValuePair<string, object?>("error.type", errorType));
}

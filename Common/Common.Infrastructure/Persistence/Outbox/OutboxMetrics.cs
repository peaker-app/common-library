using System.Diagnostics.Metrics;
using Common.Infrastructure.Observability;

namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxMetrics
{
    private const string MessagesUnit = "{message}";

    private readonly Counter<long> published;
    private readonly Counter<long> failed;

    private OutboxBacklog backlog = OutboxBacklog.Empty;

    public OutboxMetrics()
    {
        published = PeakerMetrics.Meter.CreateCounter<long>(
            "peaker.outbox.messages.published", MessagesUnit);
        failed = PeakerMetrics.Meter.CreateCounter<long>(
            "peaker.outbox.messages.failed", MessagesUnit);

        PeakerMetrics.Meter.CreateObservableGauge(
            "peaker.outbox.pending.count", () => backlog.PendingCount, MessagesUnit);
        PeakerMetrics.Meter.CreateObservableGauge(
            "peaker.outbox.parked.count", () => backlog.ParkedCount, MessagesUnit);
        PeakerMetrics.Meter.CreateObservableGauge(
            "peaker.outbox.pending.oldest_age", () => backlog.OldestPendingAge.TotalSeconds, "s");
    }

    public OutboxBacklog Backlog => backlog;

    public void RecordBacklog(OutboxBacklog value) => backlog = value;

    public void RecordPublished(string eventType) => published.Add(1, EventTypeTag(eventType));

    public void RecordFailed(string eventType) => failed.Add(1, EventTypeTag(eventType));

    private static KeyValuePair<string, object?> EventTypeTag(string eventType) =>
        new("event.type", eventType);
}

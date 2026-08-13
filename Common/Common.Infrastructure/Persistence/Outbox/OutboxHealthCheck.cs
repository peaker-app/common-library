using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Persistence.Outbox;

internal sealed class OutboxHealthCheck(OutboxMetrics metrics, IOptions<OutboxOptions> options) : IHealthCheck
{
    internal const string Name = "outbox";

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        OutboxBacklog backlog = metrics.Backlog;

        IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
        {
            ["pending"] = backlog.PendingCount,
            ["parked"] = backlog.ParkedCount,
            ["oldestAgeSeconds"] = (long)backlog.OldestPendingAge.TotalSeconds
        };

        HealthCheckResult result = backlog.ExceedsThreshold(options.Value)
            ? HealthCheckResult.Degraded("The outbox is not draining.", data: data)
            : HealthCheckResult.Healthy(data: data);

        return Task.FromResult(result);
    }
}

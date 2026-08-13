using System.Diagnostics.Metrics;

namespace Common.Infrastructure.Observability;

public sealed class CompensationMetrics
{
    private readonly Counter<long> failures;

    public CompensationMetrics() =>
        failures = PeakerMetrics.Meter.CreateCounter<long>(
            "peaker.compensation.failures", "{compensation}");

    public void RecordFailure(string assetKind) =>
        failures.Add(1, new KeyValuePair<string, object?>("asset.kind", assetKind));
}

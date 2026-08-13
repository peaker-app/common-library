using System.Diagnostics.Metrics;

namespace Common.Infrastructure.Observability;

public static class PeakerMetrics
{
    public const string MeterName = "Peaker";

    public static Meter Meter { get; } = new(MeterName);
}

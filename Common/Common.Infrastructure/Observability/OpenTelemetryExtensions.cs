using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Infrastructure.Observability;

public static class OpenTelemetryExtensions
{
    public static void AddCommonOpenTelemetry(this IHostApplicationBuilder builder, string serviceName)
    {
        Uri? otlpEndpoint = ResolveOtlpEndpoint(builder.Configuration);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => ConfigureTracing(tracing, otlpEndpoint))
            .WithMetrics(metrics => ConfigureMetrics(metrics, otlpEndpoint));
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, Uri? otlpEndpoint)
    {
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

        if (otlpEndpoint is not null)
        {
            tracing.AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint);
        }
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics, Uri? otlpEndpoint)
    {
        metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

        if (otlpEndpoint is not null)
        {
            metrics.AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpoint);
        }
    }

    private static Uri? ResolveOtlpEndpoint(IConfiguration configuration)
    {
        string? endpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        return string.IsNullOrWhiteSpace(endpoint) ? null : new Uri(endpoint);
    }
}

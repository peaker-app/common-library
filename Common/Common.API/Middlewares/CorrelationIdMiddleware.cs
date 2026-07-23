using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Common.API.Middlewares;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = ResolveCorrelationId(context);
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var value))
        {
            string header = value.ToString();

            if (!string.IsNullOrWhiteSpace(header))
            {
                return header;
            }
        }

        return Guid.CreateVersion7().ToString();
    }
}

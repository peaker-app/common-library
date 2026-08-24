using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Common.API.Caching;

public sealed class EntityTagMiddleware(RequestDelegate next, CatalogCacheOptions options)
{
    private const string WeakPrefix = "W/";
    private const string Wildcard = "*";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await next(context);
            return;
        }

        Stream original = context.Response.Body;
        using MemoryStream buffer = new();
        context.Response.Body = buffer;

        try
        {
            await next(context);
            await WriteAsync(context, buffer, original);
        }
        finally
        {
            context.Response.Body = original;
        }
    }

    private async Task WriteAsync(HttpContext context, MemoryStream buffer, Stream original)
    {
        buffer.Position = 0;

        if (context.Response.StatusCode is not StatusCodes.Status200OK)
        {
            await buffer.CopyToAsync(original);
            return;
        }

        string entityTag = ComputeEntityTag(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        context.Response.Headers.ETag = entityTag;
        context.Response.Headers.CacheControl = BuildCacheControl();

        if (IsFresh(context.Request, entityTag))
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.ContentLength = null;
            return;
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(original);
    }

    private static string ComputeEntityTag(ReadOnlySpan<byte> payload) =>
        $"W/\"{Convert.ToHexString(SHA256.HashData(payload))[..32]}\"";

    private static bool IsFresh(HttpRequest request, string entityTag)
    {
        string expected = Opaque(entityTag);

        return request.Headers.IfNoneMatch.Any(header => header is not null &&
            header.Split(',').Any(candidate =>
                candidate.Trim() == Wildcard || string.Equals(Opaque(candidate), expected, StringComparison.Ordinal)));
    }

    private static string Opaque(string entityTag)
    {
        string trimmed = entityTag.Trim();

        return trimmed.StartsWith(WeakPrefix, StringComparison.Ordinal) ? trimmed[WeakPrefix.Length..] : trimmed;
    }

    private string BuildCacheControl() => string.Create(
        CultureInfo.InvariantCulture,
        $"public, max-age={(int)options.MaxAge.TotalSeconds}, " +
        $"stale-while-revalidate={(int)options.StaleWhileRevalidate.TotalSeconds}");
}

public static class EntityTagExtensions
{
    public static IApplicationBuilder UseCatalogEntityTags(
        this IApplicationBuilder app,
        CatalogCacheOptions options) =>
        app.UseMiddleware<EntityTagMiddleware>(options);
}

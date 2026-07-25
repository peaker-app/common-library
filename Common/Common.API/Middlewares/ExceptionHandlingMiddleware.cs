using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
#pragma warning disable CA1031 // Middleware global: cualquier excepción no controlada se traduce a ProblemDetails.
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException exception)
        {
            logger.LogWarning(exception, "Rejected request to {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(context, exception.StatusCode, "The request could not be processed.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
#pragma warning restore CA1031
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

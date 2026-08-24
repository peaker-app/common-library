using System.Diagnostics;
using Common.Domain.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        long startTimestamp = Stopwatch.GetTimestamp();

        logger.LogInformation("Handling {RequestName}", requestName);

        TResponse result = await next();

        double elapsedMilliseconds = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds} ms", requestName, elapsedMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "Handled {RequestName} with error {ErrorCode} in {ElapsedMilliseconds} ms",
                requestName, result.Error.Code, elapsedMilliseconds);
        }

        return result;
    }
}

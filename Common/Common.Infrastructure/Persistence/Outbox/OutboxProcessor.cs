using System.Text.Json;
using Common.Application.Abstractions;
using Common.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessor<TContext>(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> options,
    OutboxMetrics metrics,
    ILogger<OutboxProcessor<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(options.Value.PollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
#pragma warning disable CA1031 // El worker no debe caer por un fallo transitorio: registra y reintenta en el siguiente tick.
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Outbox batch processing failed");
            }
#pragma warning restore CA1031
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TContext context = scope.ServiceProvider.GetRequiredService<TContext>();

        List<OutboxMessage> messages = await ReadEligibleAsync(context, cancellationToken);

        foreach (OutboxMessage message in messages)
        {
            await PublishAsync(scope.ServiceProvider, message, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        await RecordBacklogAsync(context, cancellationToken);
    }

    private Task<List<OutboxMessage>> ReadEligibleAsync(TContext context, CancellationToken cancellationToken)
    {
        OutboxOptions settings = options.Value;
        DateTime now = dateTimeProvider.UtcNow;

        return context.Set<OutboxMessage>()
            .Where(message => message.ProcessedAtUtc == null
                && message.AttemptCount < settings.MaxAttempts
                && (message.NextAttemptAtUtc == null || message.NextAttemptAtUtc <= now))
            .OrderBy(message => message.OccurredAtUtc)
            .Take(settings.BatchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task RecordBacklogAsync(TContext context, CancellationToken cancellationToken)
    {
        int maxAttempts = options.Value.MaxAttempts;
        IQueryable<OutboxMessage> pending = context.Set<OutboxMessage>()
            .Where(message => message.ProcessedAtUtc == null);

        int pendingCount = await pending.CountAsync(cancellationToken);
        int parkedCount = await pending.CountAsync(message => message.AttemptCount >= maxAttempts, cancellationToken);
        DateTime? oldest = await pending.MinAsync(message => (DateTime?)message.OccurredAtUtc, cancellationToken);

        RecordBacklog(new OutboxBacklog(pendingCount, parkedCount, AgeOf(oldest)));
    }

    private void RecordBacklog(OutboxBacklog backlog)
    {
        bool wasExceeding = metrics.Backlog.ExceedsThreshold(options.Value);
        metrics.RecordBacklog(backlog);

        if (!backlog.ExceedsThreshold(options.Value) || wasExceeding)
        {
            return;
        }

        logger.LogWarning(
            "Outbox backlog exceeded its threshold: {PendingCount} pending, {ParkedCount} parked, oldest {OldestAgeSeconds}s",
            backlog.PendingCount,
            backlog.ParkedCount,
            (long)backlog.OldestPendingAge.TotalSeconds);
    }

    private TimeSpan AgeOf(DateTime? oldest)
    {
        if (oldest is null)
        {
            return TimeSpan.Zero;
        }

        TimeSpan age = dateTimeProvider.UtcNow - oldest.Value;

        return age > TimeSpan.Zero ? age : TimeSpan.Zero;
    }

    private async Task PublishAsync(IServiceProvider provider, OutboxMessage message, CancellationToken cancellationToken)
    {
#pragma warning disable CA1031 // El outbox aísla el fallo por mensaje: lo contabiliza y no aborta el lote.
        try
        {
            IDomainEvent domainEvent = Deserialize(message);
            DomainEventContext eventContext = new(message.Id, message.OccurredAtUtc);

            await DomainEventDispatcher.DispatchAsync(provider, domainEvent, eventContext, cancellationToken);

            MarkProcessed(message);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            MarkFailed(message, exception);
        }
#pragma warning restore CA1031
    }

    private void MarkProcessed(OutboxMessage message)
    {
        message.ProcessedAtUtc = dateTimeProvider.UtcNow;
        message.Error = null;
        message.NextAttemptAtUtc = null;

        metrics.RecordPublished(EventTypeName(message.Type));
    }

    private void MarkFailed(OutboxMessage message, Exception exception)
    {
        OutboxOptions settings = options.Value;

        message.AttemptCount++;
        message.Error = exception.Message;
        message.NextAttemptAtUtc = NextAttemptAt(message.AttemptCount);

        metrics.RecordFailed(EventTypeName(message.Type));

        if (message.AttemptCount >= settings.MaxAttempts)
        {
            logger.LogError(
                exception,
                "Outbox message {MessageId} parked after {AttemptCount} attempts",
                message.Id,
                message.AttemptCount);

            return;
        }

        logger.LogWarning(
            exception,
            "Outbox message {MessageId} failed on attempt {AttemptCount}, retrying at {NextAttemptAtUtc}",
            message.Id,
            message.AttemptCount,
            message.NextAttemptAtUtc);
    }

    private DateTime NextAttemptAt(int attemptCount)
    {
        OutboxOptions settings = options.Value;
        double seconds = settings.RetryBackoffBase.TotalSeconds * Math.Pow(2, attemptCount - 1);
        double capped = Math.Min(seconds, settings.RetryBackoffCap.TotalSeconds);

        return dateTimeProvider.UtcNow.AddSeconds(capped);
    }

    private static string EventTypeName(string type)
    {
        int comma = type.IndexOf(',', StringComparison.Ordinal);
        string fullName = comma < 0 ? type : type[..comma];
        int dot = fullName.LastIndexOf('.');

        return dot < 0 ? fullName : fullName[(dot + 1)..];
    }

    private static IDomainEvent Deserialize(OutboxMessage message)
    {
        Type type = Type.GetType(message.Type)
            ?? throw new InvalidOperationException($"Unknown outbox message type '{message.Type}'.");

        return (IDomainEvent)JsonSerializer.Deserialize(message.Content, type, SerializerOptions)!;
    }
}

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
    IOptions<OutboxOptions> options,
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

        List<OutboxMessage> messages = await context.Set<OutboxMessage>()
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .Take(options.Value.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (OutboxMessage message in messages)
        {
            await PublishAsync(scope.ServiceProvider, message, cancellationToken);
        }

        if (messages.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PublishAsync(IServiceProvider provider, OutboxMessage message, CancellationToken cancellationToken)
    {
#pragma warning disable CA1031 // El outbox aísla el fallo por mensaje: registra el motivo y no aborta el lote.
        try
        {
            IDomainEvent domainEvent = Deserialize(message);
            await DomainEventDispatcher.DispatchAsync(provider, domainEvent, cancellationToken);
            message.ProcessedAtUtc = provider.GetRequiredService<IDateTimeProvider>().UtcNow;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to process outbox message {MessageId}", message.Id);
            message.Error = exception.Message;
        }
#pragma warning restore CA1031
    }

    private static IDomainEvent Deserialize(OutboxMessage message)
    {
        Type type = Type.GetType(message.Type)
            ?? throw new InvalidOperationException($"Unknown outbox message type '{message.Type}'.");

        return (IDomainEvent)JsonSerializer.Deserialize(message.Content, type, SerializerOptions)!;
    }
}

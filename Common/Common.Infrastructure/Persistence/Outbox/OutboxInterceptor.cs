using Common.Application.Abstractions;
using Common.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxInterceptor(IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AddOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddOutboxMessages(DbContext context)
    {
        DateTime occurredAtUtc = dateTimeProvider.UtcNow;

        List<OutboxMessage> messages = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(DrainDomainEvents)
            .Select(domainEvent => ToOutboxMessage(domainEvent, occurredAtUtc))
            .ToList();

        if (messages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(messages);
        }
    }

    private static IReadOnlyCollection<IDomainEvent> DrainDomainEvents(AggregateRoot aggregate)
    {
        IReadOnlyCollection<IDomainEvent> domainEvents = [.. aggregate.DomainEvents];
        aggregate.ClearDomainEvents();
        return domainEvents;
    }

    private static OutboxMessage ToOutboxMessage(IDomainEvent domainEvent, DateTime occurredAtUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        Type = domainEvent.GetType().AssemblyQualifiedName!,
        Content = OutboxSerializer.Serialize(domainEvent),
        OccurredAtUtc = occurredAtUtc
    };
}

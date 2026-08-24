using Common.Domain.Abstractions;

namespace Common.Application.Abstractions;

/// <summary>
/// Identidad estable del mensaje de outbox que transporta el evento de dominio. Sobrevive a las
/// reentregas, de modo que el consumidor puede descartar duplicados contra <c>processed_messages</c>.
/// </summary>
public sealed record DomainEventContext(Guid MessageId, DateTime OccurredAtUtc);

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken);
}

using System.Reflection;
using Common.Application.Abstractions;
using Common.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Persistence.Outbox;

internal static class DomainEventDispatcher
{
    private const string HandleMethodName = nameof(IDomainEventHandler<IDomainEvent>.Handle);

    public static async Task DispatchAsync(
        IServiceProvider provider,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        MethodInfo handleMethod = handlerType.GetMethod(HandleMethodName)!;

        foreach (object handler in provider.GetServices(handlerType).OfType<object>())
        {
            await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
        }
    }
}

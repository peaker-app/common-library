using System.Text.Json;
using Common.Domain.Abstractions;

namespace Common.Infrastructure.Persistence.Outbox;

public static class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options);

    public static IDomainEvent Deserialize(OutboxMessage message)
    {
        Type type = Type.GetType(message.Type)
            ?? throw new InvalidOperationException($"Unknown outbox message type '{message.Type}'.");

        return (IDomainEvent)JsonSerializer.Deserialize(message.Content, type, Options)!;
    }
}

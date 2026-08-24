namespace Common.Contracts.Abstractions;

public abstract record IntegrationEvent
{
    /// <summary>
    /// Identidad del mensaje de cara al consumidor. La fija el outbox con el identificador de la
    /// fila publicada: si se genera aquí, cada reentrega estrenaría uno y la comprobación contra
    /// <c>processed_messages</c> dejaría de reconocer el duplicado.
    /// </summary>
    public required Guid MessageId { get; init; }

    public Guid CorrelationId { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public int Version { get; init; } = 1;
}

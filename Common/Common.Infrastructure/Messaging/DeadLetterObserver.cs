using MassTransit;

namespace Common.Infrastructure.Messaging;

public sealed class DeadLetterObserver(MessagingMetrics metrics) : IReceiveObserver
{
    public Task PreReceive(ReceiveContext context) => Task.CompletedTask;

    public Task PostReceive(ReceiveContext context) => Task.CompletedTask;

    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
        where T : class => Task.CompletedTask;

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
        where T : class => Task.CompletedTask;

    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        metrics.RecordDeadLettered(QueueOf(context), exception.GetType().Name);

        return Task.CompletedTask;
    }

    private static string QueueOf(ReceiveContext context) =>
        context.InputAddress.AbsolutePath.TrimStart('/');
}

using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using MassTransit;

namespace EmployeeService.Consumers;

public class TransientFailureConsumer(ILogger<TransientFailureConsumer> logger)
    : IConsumer<TransientFailureDemo>
{
    // Demo-only process memory. For repeatable distributed tests, control this
    // behavior from a dedicated test fake rather than using static state.
    private static readonly Dictionary<Guid, int> Attempts = new();
    private static readonly object Sync = new();

    public Task Consume(ConsumeContext<TransientFailureDemo> context)
    {
        int attempt;
        lock (Sync)
        {
            Attempts.TryGetValue(context.Message.MessageId, out attempt);
            attempt++;
            Attempts[context.Message.MessageId] = attempt;
        }

        logger.LogInformation("Message {MessageId}, attempt {Attempt}",
            context.Message.MessageId, attempt);

        if (attempt <= context.Message.FailFirstAttempts)
            throw new InvalidOperationException($"Simulated transient failure on attempt {attempt}.");

        logger.LogInformation("Message {MessageId} processed successfully.",
            context.Message.MessageId);

        lock (Sync)
            Attempts.Remove(context.Message.MessageId);

        return Task.CompletedTask;
    }
}

using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using MassTransit;

namespace ProductService.Consumers;

public class ProductNotificationProcessedConsumer(ILogger<ProductNotificationProcessedConsumer> logger)
    : IConsumer<ProductNotificationProcessed>
{
    public Task Consume(ConsumeContext<ProductNotificationProcessed> context)
    {
        logger.LogInformation(
            "EmployeeService processed Product {ProductId}; source message {SourceMessageId}",
            context.Message.ProductId,
            context.Message.SourceMessageId);

        return Task.CompletedTask;
    }
}

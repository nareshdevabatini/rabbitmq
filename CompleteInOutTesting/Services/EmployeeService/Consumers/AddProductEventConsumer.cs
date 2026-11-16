using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using EmployeeService.Infrastruture;
using EmployeeService.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployeeService.Consumers;

public sealed class AddProductEventConsumer(EmployeeDBContext dbContext)
    : IConsumer<AddProductEvent>
{
    public async Task Consume(ConsumeContext<AddProductEvent> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("MessageId is required for Inbox deduplication.");

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        var alreadyProcessed = await dbContext.InboxMessages
            .AnyAsync(x => x.MessageId == messageId, context.CancellationToken);

        if (alreadyProcessed)
        {
            await transaction.CommitAsync(context.CancellationToken);
            return;
        }

        dbContext.InboxMessages.Add(new InboxMessage
        {
            MessageId = messageId,
            ReceivedAtUtc = DateTime.UtcNow
        });

        dbContext.ProductNotifications.Add(new EmployeeProductNotification
        {
            ProductId = context.Message.ProductId,
            SourceMessageId = messageId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        var outgoingEvent = new ProductNotificationProcessed(
            context.Message.ProductId,
            messageId);

        dbContext.EmployeeOutboxMessages.Add(new EmployeeOutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = nameof(ProductNotificationProcessed),
            Payload = JsonSerializer.Serialize(outgoingEvent),
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
        await transaction.CommitAsync(context.CancellationToken);
    }
}

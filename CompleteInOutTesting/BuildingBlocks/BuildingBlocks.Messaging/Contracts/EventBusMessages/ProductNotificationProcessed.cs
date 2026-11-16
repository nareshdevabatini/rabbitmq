namespace BuildingBlocks.Messaging.Contracts.EventBusMessages;

public record ProductNotificationProcessed(int ProductId, Guid SourceMessageId);

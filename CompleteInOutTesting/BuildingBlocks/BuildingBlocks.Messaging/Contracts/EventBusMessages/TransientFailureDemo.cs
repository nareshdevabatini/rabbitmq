namespace BuildingBlocks.Messaging.Contracts.EventBusMessages;

public record TransientFailureDemo(Guid MessageId, int FailFirstAttempts);

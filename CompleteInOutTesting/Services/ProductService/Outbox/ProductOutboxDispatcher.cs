using System.Text.Json;
using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;

namespace ProductService.Outbox;

public sealed class ProductOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductOutboxDispatcher> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Product Outbox dispatch loop failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchBatch(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                switch (message.MessageType)
                {
                    case "AddProductEvent":
                        var productEvent = JsonSerializer.Deserialize<AddProductEvent>(message.Payload)
                            ?? throw new InvalidOperationException("Invalid AddProductEvent payload.");
                        await publish.Publish(productEvent, cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown outbox message type: {message.MessageType}");
                }

                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Published outbox message {OutboxMessageId}", message.Id);
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.LastError = ex.Message;
                await db.SaveChangesAsync(cancellationToken);
                logger.LogError(ex, "Failed to publish outbox message {OutboxMessageId}", message.Id);
                // Do not mark it processed; it will be retried on a later polling cycle.
            }
        }
    }
}

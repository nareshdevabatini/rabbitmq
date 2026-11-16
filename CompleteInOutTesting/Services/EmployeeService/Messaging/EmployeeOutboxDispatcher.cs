using System.Text.Json;
using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using EmployeeService.Infrastruture;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Messaging;

public sealed class EmployeeOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<EmployeeOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<EmployeeDBContext>();
                var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var batch = await db.EmployeeOutboxMessages
                    .Where(x => x.ProcessedAtUtc == null)
                    .OrderBy(x => x.CreatedAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in batch)
                {
                    try
                    {
                        if (message.MessageType != nameof(ProductNotificationProcessed))
                            throw new NotSupportedException($"Unknown message type {message.MessageType}");

                        var evt = JsonSerializer.Deserialize<ProductNotificationProcessed>(message.Payload)
                            ?? throw new InvalidOperationException("Invalid ProductNotificationProcessed payload.");

                        await publish.Publish(evt, stoppingToken);
                        message.ProcessedAtUtc = DateTime.UtcNow;
                        message.LastError = null;
                    }
                    catch (Exception ex)
                    {
                        message.Attempts++;
                        message.LastError = ex.Message;
                        logger.LogError(ex, "Employee outbox message {Id} failed.", message.Id);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Employee Outbox polling failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}

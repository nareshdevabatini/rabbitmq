using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;

namespace OrderService.Consumers;

public class InventoryConsumer(ILogger<InventoryConsumer> logger) : 
    IConsumer<ReserveInventory>,
    IConsumer<ReleaseInventory>
{
    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        logger.LogInformation("Reserving {Quantity} units for product {ProductId}, order {OrderId}",
            context.Message.Quantity, context.Message.ProductId, context.Message.OrderId);

        if (context.Message.ProductId <= 0 || context.Message.Quantity <= 0)
        {
            await context.Publish(new InventoryReservationFailed(
                context.Message.OrderId, "Invalid product or quantity."));
            return;
        }

        await context.Publish(new InventoryReserved(context.Message.OrderId));
    }

    public async Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        logger.LogWarning("Compensating inventory reservation for order {OrderId}", context.Message.OrderId);
        // A real Inventory service would persist a reservation release idempotently here.
        await context.Publish(new InventoryReleased(context.Message.OrderId));
    }
}

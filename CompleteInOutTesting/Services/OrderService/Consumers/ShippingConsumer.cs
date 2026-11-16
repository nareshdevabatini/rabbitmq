using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;

namespace OrderService.Consumers;

public class ShippingConsumer(ILogger<ShippingConsumer> logger) : IConsumer<CreateShipment>
{
    public async Task Consume(ConsumeContext<CreateShipment> context)
    {
        logger.LogInformation("Creating shipment for order {OrderId}", context.Message.OrderId);

        if (context.Message.SimulateFailure)
        {
            await context.Publish(new ShipmentFailed(
                context.Message.OrderId, "Simulated carrier/service failure."));
            return;
        }

        await context.Publish(new ShipmentCreated(
            context.Message.OrderId, $"SHIP-{context.Message.OrderId:N}"));
    }
}

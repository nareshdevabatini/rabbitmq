using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;

namespace OrderService.Consumers;

public class PaymentConsumer(ILogger<PaymentConsumer> logger) :
    IConsumer<AuthorizePayment>,
    IConsumer<VoidPayment>
{
    public async Task Consume(ConsumeContext<AuthorizePayment> context)
    {
        logger.LogInformation("Authorizing payment for order {OrderId}, amount {Amount}",
            context.Message.OrderId, context.Message.Amount);

        // Deterministic failure branch for the learning exercise.
        if (context.Message.Amount > 50000m || context.Message.Amount <= 0)
        {
            await context.Publish(new PaymentFailed(
                context.Message.OrderId, "Payment was declined by the demo rule."));
            return;
        }

        await context.Publish(new PaymentAuthorized(
            context.Message.OrderId, $"TXN-{context.Message.OrderId:N}"));
    }

    public async Task Consume(ConsumeContext<VoidPayment> context)
    {
        logger.LogWarning("Voiding payment {TransactionId} for order {OrderId}",
            context.Message.TransactionId, context.Message.OrderId);
        // A real Payment service would void/refund idempotently and persist its result.
        await context.Publish(new PaymentVoided(context.Message.OrderId));
    }
}

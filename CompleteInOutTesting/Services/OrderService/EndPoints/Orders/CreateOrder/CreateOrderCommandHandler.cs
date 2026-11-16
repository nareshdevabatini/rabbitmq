using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.EndPoints.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(
    OrderSagaDbContext dbContext,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ProductId <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.ProductId), "ProductId must be positive.");

        if (command.Quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.Quantity), "Quantity must be positive.");

        if (command.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.Amount), "Amount must be positive.");

        var orderId = Guid.NewGuid();

        // The Bus Outbox captures SubmitOrder in this DbContext scope.
        // OrderRequest and the outgoing message are saved atomically.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.OrderRequests.Add(new OrderRequest
        {
            OrderId = orderId,
            ProductId = command.ProductId,
            Quantity = command.Quantity,
            Amount = command.Amount,
            CreatedAtUtc = DateTime.UtcNow
        });

        await publishEndpoint.Publish(
            new SubmitOrder(
                orderId,
                command.ProductId,
                command.Quantity,
                command.Amount,
                command.SimulateShippingFailure),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CreateOrderResult(orderId);
    }
}

using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Sagas;

namespace OrderService.EndPoints.Orders.GetOrderById;

public sealed class GetOrderByIdQueryHandler(OrderSagaDbContext dbContext)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.OrderRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == query.OrderId, cancellationToken);

        if (order is null)
        {
            return new GetOrderByIdResult(
                Found: false,
                OrderId: query.OrderId,
                Status: "NotFound");
        }

        var saga = await dbContext.Set<OrderState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CorrelationId == query.OrderId, cancellationToken);

        // The request is committed before the asynchronous Saga consumes SubmitOrder.
        // While the Saga row has not yet been created, report Submitted rather than 404.
        return new GetOrderByIdResult(
            Found: true,
            OrderId: order.OrderId,
            Status: saga?.CurrentState ?? "Submitted",
            ProductId: order.ProductId,
            Quantity: order.Quantity,
            Amount: order.Amount,
            FailureReason: saga?.FailureReason);
    }
}

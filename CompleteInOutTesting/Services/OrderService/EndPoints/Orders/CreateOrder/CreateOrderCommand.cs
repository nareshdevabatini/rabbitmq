using BuildingBlocks.CQRS;

namespace OrderService.EndPoints.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    int ProductId,
    int Quantity,
    decimal Amount,
    bool SimulateShippingFailure = false)
    : ICommand<CreateOrderResult>;

public sealed record CreateOrderResult(Guid OrderId);

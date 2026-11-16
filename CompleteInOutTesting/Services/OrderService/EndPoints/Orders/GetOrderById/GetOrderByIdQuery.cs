using BuildingBlocks.CQRS;

namespace OrderService.EndPoints.Orders.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId)
    : IQuery<GetOrderByIdResult>;

public sealed record GetOrderByIdResult(
    bool Found,
    Guid OrderId,
    string Status,
    int? ProductId = null,
    int? Quantity = null,
    decimal? Amount = null,
    string? FailureReason = null);

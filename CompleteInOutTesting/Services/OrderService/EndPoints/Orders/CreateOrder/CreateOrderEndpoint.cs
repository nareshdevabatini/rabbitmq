using Carter;
using MediatR;

namespace OrderService.EndPoints.Orders.CreateOrder;

public sealed record CreateOrderRequest(
    int ProductId,
    int Quantity,
    decimal Amount,
    bool SimulateShippingFailure = false);

public sealed class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateOrderCommand(
                    request.ProductId,
                    request.Quantity,
                    request.Amount,
                    request.SimulateShippingFailure),
                cancellationToken);

            return Results.Accepted(
                $"/orders/{result.OrderId}",
                new { result.OrderId, Status = "Submitted" });
        })
        .WithName("CreateOrder")
        .WithTags("Orders");
    }
}

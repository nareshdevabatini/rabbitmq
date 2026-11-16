using Carter;
using MediatR;

namespace OrderService.EndPoints.Orders.GetOrderById;

public sealed class GetOrderByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}", async (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetOrderByIdQuery(orderId),
                cancellationToken);

            return result.Found
                ? Results.Ok(result)
                : Results.NotFound();
        })
        .WithName("GetOrderById")
        .WithTags("Orders");
    }
}

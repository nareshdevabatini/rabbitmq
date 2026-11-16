using Carter;
using MediatR;
using ProductService.Model;

namespace ProductService.EndPoints.Products.GetAll;

public sealed record GetAllProductResponse(IEnumerable<Product> Products);

public sealed class GetAllProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductQuery(), cancellationToken);
            return Results.Ok(new GetAllProductResponse(result.Products));
        });
    }
}

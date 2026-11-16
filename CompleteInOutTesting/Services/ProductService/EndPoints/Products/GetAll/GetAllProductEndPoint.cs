using Carter;
using Mapster;
using MediatR;
using ProductService.Model;

namespace ProductService.EndPoints.Products.GetAll;

//public record GetAllProductRequest(CancellationToken cancellationToken);
public record GetAllProductResponse(IEnumerable<Product> Products);
public class GetAllProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (CancellationToken cancellationToken, ISender sender) =>
        {
            var result = await sender.Send(new GetProductQuery(cancellationToken));

            var response = result.Adapt<GetProductResult>();

            return Results.Ok(response);
        });
    }
}

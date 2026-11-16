using Carter;
using Mapster;
using MediatR;
using ProductService.Model;

namespace ProductService.EndPoints.Products.CreateProduct;

public record CreateProductRequest(Product Product, CancellationToken cancellationToken);
public record CreateProductResponse(bool IsSuccess);
public class CreateProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<ProductCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<CreateProductResponse>();

            return Results.Ok(response);
        });
    }
}

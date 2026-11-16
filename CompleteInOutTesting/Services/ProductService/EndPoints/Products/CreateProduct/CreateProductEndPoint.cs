using Carter;
using Mapster;
using MediatR;
using ProductService.Model;

namespace ProductService.EndPoints.Products.CreateProduct;

public sealed record CreateProductRequest(Product Product);
public sealed record CreateProductResponse(bool IsSuccess);

public sealed class CreateProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (
            CreateProductRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = request.Adapt<ProductCommand>();
            var result = await sender.Send(command, cancellationToken);
            var response = result.Adapt<CreateProductResponse>();

            return Results.Ok(response);
        });
    }
}

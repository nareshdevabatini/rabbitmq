using BuildingBlocks.CQRS;
using ProductService.Model;
using ProductService.Repository;

namespace ProductService.EndPoints.Products.GetAll;

public sealed record GetProductQuery : IQuery<GetProductResult>;
public sealed record GetProductResult(IEnumerable<Product> Products);

public sealed class GetAllProductHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductQuery, GetProductResult>
{
    public async Task<GetProductResult> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllProducts(cancellationToken);
        return new GetProductResult(products);
    }
}

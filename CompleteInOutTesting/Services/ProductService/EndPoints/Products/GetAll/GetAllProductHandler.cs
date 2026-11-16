using BuildingBlocks.CQRS;
using ProductService.Model;
using ProductService.Repository;

namespace ProductService.EndPoints.Products.GetAll;

public record GetProductQuery(CancellationToken cancellationToken) : IQuery<GetProductResult>;
public record GetProductResult(IEnumerable<Product> Products);

public class GetAllProductHandler(IProductRepository productRepository) : IQueryHandler<GetProductQuery, GetProductResult>
{
    public async Task<GetProductResult> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllProducts(cancellationToken);
        return new GetProductResult(products);
    }
}


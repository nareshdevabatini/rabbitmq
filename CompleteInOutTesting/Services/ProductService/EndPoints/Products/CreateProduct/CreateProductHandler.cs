using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using MassTransit;
using ProductService.Model;
using ProductService.Repository;

namespace ProductService.EndPoints.Products.CreateProduct;

public record ProductCommand(Product Product) : ICommand<ProductResult>;
public record ProductResult(bool IsSuccess);
public class CreateProductCommandHandler(IProductRepository _repository, IPublishEndpoint publishEndpoint) : ICommandHandler<ProductCommand, ProductResult>
{
    public async Task<ProductResult> Handle(ProductCommand request, CancellationToken cancellationToken)
    {
        await _repository.AddProduct(request.Product, cancellationToken);
        if (request.Product.Id <= 0)
        {
            return new ProductResult(true);
        }
        var eventMessage = new AddProductEvent(request.Product.Id);
        await publishEndpoint.Publish(eventMessage, cancellationToken);
        return new ProductResult(true);
    }
}


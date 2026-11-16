using System.Text.Json;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using ProductService.Model;
using ProductService.Outbox;

namespace ProductService.EndPoints.Products.CreateProduct;

public record ProductCommand(Product Product) : ICommand<ProductResult>;
public record ProductResult(bool IsSuccess);

public class CreateProductCommandHandler(ProductDbContext dbContext)
    : ICommandHandler<ProductCommand, ProductResult>
{
    public async Task<ProductResult> Handle(
        ProductCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Products.Add(request.Product);
        await dbContext.SaveChangesAsync(cancellationToken); // Assigns Product.Id inside the transaction.

        var integrationEvent = new AddProductEvent(request.Product.Id);

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = nameof(AddProductEvent),
            Payload = JsonSerializer.Serialize(integrationEvent),
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ProductResult(true);
    }
}

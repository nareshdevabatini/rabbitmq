using BuildingBlocks.Messaging.Contracts.EventBusMessages;
using MassTransit;

namespace EmployeeService.EndPoints.EmployeeProduct.AddEmployeeProduct;

public class AddEmployeeProductHandler : IConsumer<AddProductEvent>
{
    public async Task Consume(ConsumeContext<AddProductEvent> context)
    {
        var productEvent = context.Message;

        Console.WriteLine($"Received Notification Event: Recipient={productEvent.ProductId}, ");

        await Task.CompletedTask;
    }
}

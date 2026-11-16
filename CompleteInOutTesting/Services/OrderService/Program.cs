

using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Consumers;
using OrderService.Data;
using OrderService.Sagas;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["MessageBroker:Host"] ?? "amqp://localhost:5672";
var rabbitUser = builder.Configuration["MessageBroker:UserName"] ?? "ecommerce";
var rabbitPassword = builder.Configuration["MessageBroker:Password"] ?? "ecommerce123";
var connectionString = builder.Configuration.GetConnectionString("OrderDb")
    ?? "Data Source=orders.db";

builder.Services.AddDbContext<OrderSagaDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrderSagaDbContext>();
            r.UseSqlite();
        });

    x.AddConsumers(typeof(InventoryConsumer).Assembly);

    // The Bus Outbox stores messages published from the API in OrderSagaDbContext.
    // The Consumer Outbox stores incoming-message deduplication and outgoing messages
    // for receive endpoints that use this DbContext.
    x.AddEntityFrameworkOutbox<OrderSagaDbContext>(o =>
    {
        o.UseSqlite();
        o.UseBusOutbox();
    });

    x.AddConfigureEndpointsCallback((context, name, endpoint) =>
    {
        endpoint.UseMessageRetry(r => r.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(10),
            intervalDelta: TimeSpan.FromSeconds(2)));

        endpoint.UseEntityFrameworkOutbox<OrderSagaDbContext>(context);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitHost), host =>
        {
            host.Username(rabbitUser);
            host.Password(rabbitPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Create and apply the initial EF migration before running this service.
// See README-START-HERE.md for the dotnet ef commands.

app.MapPost("/orders", async (
    CreateOrderRequest request,
    OrderSagaDbContext db,
    IPublishEndpoint publishEndpoint,
    CancellationToken cancellationToken) =>
{
    if (request.ProductId < 0 || request.Quantity <= 0 || request.Amount <= 0)
        return Results.BadRequest("ProductId must be >= 0, Quantity and Amount must be positive.");

    var orderId = Guid.NewGuid();

    db.OrderRequests.Add(new OrderRequest
    {
        OrderId = orderId,
        ProductId = request.ProductId,
        Quantity = request.Quantity,
        Amount = request.Amount,
        CreatedAtUtc = DateTime.UtcNow
    });

    // Bus Outbox captures this publication in the scoped OrderSagaDbContext.
    await publishEndpoint.Publish(new SubmitOrder(
        orderId,
        request.ProductId,
        request.Quantity,
        request.Amount,
        request.SimulateShippingFailure), cancellationToken);

    await db.SaveChangesAsync(cancellationToken);

    return Results.Accepted($"/orders/{orderId}", new { orderId, status = "Submitted" });
});

app.MapGet("/orders/{orderId:guid}", async (
    Guid orderId,
    OrderSagaDbContext db,
    CancellationToken cancellationToken) =>
{
    var state = await db.Set<OrderState>()
        .AsNoTracking()
        .SingleOrDefaultAsync(x => x.CorrelationId == orderId, cancellationToken);

    return state is null
        ? Results.NotFound()
        : Results.Ok(new
        {
            state.CorrelationId,
            state.CurrentState,
            state.ProductId,
            state.Quantity,
            state.Amount,
            state.FailureReason
        });
});

app.Run();

public record CreateOrderRequest(
    int ProductId,
    int Quantity,
    decimal Amount,
    bool SimulateShippingFailure = false);

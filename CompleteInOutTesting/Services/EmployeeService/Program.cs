
using BuildingBlocks.Messaging.Configuration;
using Carter;
using EmployeeService.Infrastruture;
using EmployeeService.Messaging;
using EmployeeService.Respository;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.Decorate<IEmployeeRepository, CachedEmployeeRepository>();
builder.Services.AddHostedService<EmployeeOutboxDispatcher>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")!;
});

//MassTransit 
builder.Services.AddMessageBroker(builder.Configuration, assembly);

// Add services to the container.
builder.Services.AddDbContext<EmployeeDBContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

//app.MapPost("/retry-demo", async (
//    RetryDemoRequest request,
//    IPublishEndpoint publishEndpoint,
//    CancellationToken cancellationToken) =>
//{
//    var messageId = Guid.NewGuid();
//    await publishEndpoint.Publish(
//        new TransientFailureDemo(messageId, request.FailFirstAttempts),
//        cancellationToken);

//    return Results.Accepted(value: new { messageId });
//});

// Configure the HTTP request pipeline.
app.MapCarter();

app.UseHttpsRedirection();


app.Run();


using Carter;
using OrderService.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrderService(
    builder.Configuration,
    typeof(Program).Assembly);

var app = builder.Build();

app.MapCarter();

app.Run();

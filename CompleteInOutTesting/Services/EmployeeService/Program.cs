
using Carter;
using EmployeeService.Infrastruture;
using EmployeeService.Respository;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Messaging.Configuration;

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

// Configure the HTTP request pipeline.
app.MapCarter();

app.UseHttpsRedirection();


app.Run();


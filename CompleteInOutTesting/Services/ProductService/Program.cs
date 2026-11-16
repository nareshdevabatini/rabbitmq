using BuildingBlocks.ExceptionHandling.Extension;
using BuildingBlocks.ExceptionHandling.Handler;
using Carter;
using Microsoft.EntityFrameworkCore;
using ProductService.Repository;
using BuildingBlocks.Messaging.Configuration;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddDbContext<ProductService.Infrastructure.ProductDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//MassTransit 
builder.Services.AddMessageBroker(builder.Configuration, assembly);
var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler((options) => { });
//app.Use(async (content, next) =>
//{
//    Console.WriteLine("Hello World!");
//    await next();
//    Console.WriteLine("Goodbye World!");
//});
//app.Map("/admin", adminApp =>
//{
//    adminApp.Use(async (context, next) =>
//    {
//        //throw new Exception("Admin error");
//        await context.Response.WriteAsync("Admin");
//        Console.WriteLine("Admin middleware");
//        await next();
//    });

//    adminApp.Run(async context =>
//    {
//        await context.Response.WriteAsync("Admin dashboard");
//    });
//});
//app.Use(async (content, next) =>
//{
//    Console.WriteLine("Hello World! 2");
//    await next();
//    Console.WriteLine("Goodbye World! 2");
//});
app.UseMiddleware<NotFoundStatusCodeMiddleware>();
app.UseHttpsRedirection();
app.Run();

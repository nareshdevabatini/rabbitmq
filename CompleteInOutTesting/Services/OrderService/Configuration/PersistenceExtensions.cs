using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Configuration;

public static class PersistenceExtensions
{
    public static IServiceCollection AddOrderPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderDb")
            ?? "Data Source=orders.db";

        services.AddDbContext<OrderSagaDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}

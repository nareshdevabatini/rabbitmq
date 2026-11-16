using System.Reflection;
using Carter;
using MediatR;

namespace OrderService.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderService(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        services.AddCarter();
        services.AddMediatR(options =>
            options.RegisterServicesFromAssembly(assembly));

        services.AddOrderPersistence(configuration);
        services.AddOrderMessaging(configuration, assembly);

        return services;
    }
}

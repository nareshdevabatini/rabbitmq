using System.Reflection;
using MassTransit;
using OrderService.Data;
using OrderService.Sagas;

namespace OrderService.Configuration;

public static class MessagingExtensions
{
    public static IServiceCollection AddOrderMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        var rabbitHost = configuration["MessageBroker:Host"]
            ?? "amqp://localhost:5672";
        var rabbitUser = configuration["MessageBroker:UserName"]
            ?? "ecommerce";
        var rabbitPassword = configuration["MessageBroker:Password"]
            ?? "ecommerce123";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(assembly);

            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(repository =>
                {
                    repository.ExistingDbContext<OrderSagaDbContext>();
                    repository.UseSqlite();
                });

            // Bus Outbox: outgoing messages published from API scopes are persisted
            // in OrderSagaDbContext and committed with the business transaction.
            x.AddEntityFrameworkOutbox<OrderSagaDbContext>(options =>
            {
                options.UseSqlite();
                options.UseBusOutbox();
            });

            x.AddConfigureEndpointsCallback((context, endpointName, endpoint) =>
            {
                endpoint.UseMessageRetry(retry => retry.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(10),
                    intervalDelta: TimeSpan.FromSeconds(2)));

                // Consumer Outbox: records incoming-message state and outgoing
                // messages in the same local database transaction.
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

        return services;
    }
}

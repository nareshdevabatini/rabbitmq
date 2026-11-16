using MassTransit;

namespace BuildingBlocks.Messaging.Configuration;

public static class RetryConfiguration
{
    /// <summary>
    /// Short, bounded retries for transient consumer failures.
    /// Attach this to each receive endpoint before ConfigureEndpoints is called.
    /// </summary>
    public static void UseStandardTransientRetry(this IReceiveEndpointConfigurator endpoint)
    {
        endpoint.UseMessageRetry(retry => retry.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(10),
            intervalDelta: TimeSpan.FromSeconds(2)));
    }

    /// <summary>
    /// Requires RabbitMQ delayed-message-exchange plugin when used with RabbitMQ.
    /// It is intentionally separate from immediate retry.
    /// </summary>
    public static void UseOptionalDelayedRedelivery(this IReceiveEndpointConfigurator endpoint)
    {
        endpoint.UseDelayedRedelivery(redelivery => redelivery.Intervals(
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30)));
    }
}

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var mode = args.FirstOrDefault()?.ToLowerInvariant() ?? "direct";

var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "ecommerce",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "ecommerce123",
    DispatchConsumersAsync = false
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

switch (mode)
{
    case "direct":
        Direct(channel);
        break;
    case "fanout":
        Fanout(channel);
        break;
    case "topic":
        Topic(channel);
        break;
    case "headers":
        Headers(channel);
        break;
    case "dlx":
        DeadLetter(channel);
        break;
    case "ttl-redelivery":
        TtlRedelivery(channel);
        break;
    default:
        Console.WriteLine("Modes: direct | fanout | topic | headers | dlx | ttl-redelivery");
        break;
}

static void Direct(IModel channel)
{
    const string exchange = "lab.direct";
    const string queue = "lab.direct.employee-created";
    const string key = "employee.created";

    channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);
    channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
    channel.QueueBind(queue, exchange, key);

    Publish(channel, exchange, key, "Direct: employee created");
    Publish(channel, exchange, "employee.deleted", "This message does not match the binding");

    Console.WriteLine($"Published two messages. Queue '{queue}' should receive only the employee.created message.");
}

static void Fanout(IModel channel)
{
    const string exchange = "lab.fanout";

    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);

    foreach (var queue in new[] { "lab.fanout.audit", "lab.fanout.reporting" })
    {
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey: "");
    }

    Publish(channel, exchange, "ignored.routing.key", "Fanout broadcast to every bound queue");
    Console.WriteLine("Both bound queues should receive a copy. Fanout ignores the routing key.");
}

static void Topic(IModel channel)
{
    const string exchange = "lab.topic";

    channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

    Bind(channel, exchange, "lab.topic.sales-all", "sales.#");
    Bind(channel, exchange, "lab.topic.sales-audit", "sales.audit.*");
    Bind(channel, exchange, "lab.topic.employee", "employee.*");

    Publish(channel, exchange, "sales.audit.completed", "Sales audit completed");
    Publish(channel, exchange, "sales.order.created", "Sales order created");
    Publish(channel, exchange, "employee.created", "Employee created");

    Console.WriteLine("Inspect the three queues to compare topic pattern matching.");
}

static void Headers(IModel channel)
{
    const string exchange = "lab.headers";
    const string queue = "lab.headers.priority";

    channel.ExchangeDeclare(exchange, ExchangeType.Headers, durable: true);
    channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

    var bindingArguments = new Dictionary<string, object>
    {
        ["x-match"] = "all",
        ["event-type"] = "employee-created",
        ["priority"] = "high"
    };

    channel.QueueBind(queue, exchange, routingKey: "", arguments: bindingArguments);

    var matchingHeaders = new Dictionary<string, object>
    {
        ["event-type"] = "employee-created",
        ["priority"] = "high"
    };

    var nonMatchingHeaders = new Dictionary<string, object>
    {
        ["event-type"] = "employee-created",
        ["priority"] = "low"
    };

    Publish(channel, exchange, "", "Headers match: should route", matchingHeaders);
    Publish(channel, exchange, "", "Headers mismatch: should not route", nonMatchingHeaders);

    Console.WriteLine($"Queue '{queue}' is bound with x-match=all and two header values.");
}

static void DeadLetter(IModel channel)
{
    const string workExchange = "lab.dlx.work";
    const string deadExchange = "lab.dlx.dead";
    const string workQueue = "lab.dlx.work-queue";
    const string deadQueue = "lab.dlx.dead-queue";
    const string workKey = "work";
    const string deadKey = "dead";

    channel.ExchangeDeclare(workExchange, ExchangeType.Direct, durable: true);
    channel.ExchangeDeclare(deadExchange, ExchangeType.Direct, durable: true);
    channel.QueueDeclare(deadQueue, durable: true, exclusive: false, autoDelete: false);
    channel.QueueBind(deadQueue, deadExchange, deadKey);

    var args = new Dictionary<string, object>
    {
        ["x-dead-letter-exchange"] = deadExchange,
        ["x-dead-letter-routing-key"] = deadKey
    };

    channel.QueueDeclare(workQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);
    channel.QueueBind(workQueue, workExchange, workKey);

    Publish(channel, workExchange, workKey, "This message will be rejected and dead-lettered");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (_, ea) =>
    {
        Console.WriteLine("Work consumer received message and rejects it without requeue.");
        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
    };

    channel.BasicConsume(workQueue, autoAck: false, consumer);
    Console.WriteLine("Waiting up to 5 seconds for rejection and DLX routing...");
    Thread.Sleep(TimeSpan.FromSeconds(5));

    var deadLetter = channel.BasicGet(deadQueue, autoAck: false);
    if (deadLetter is not null)
    {
        Console.WriteLine($"DLQ received: {Encoding.UTF8.GetString(deadLetter.Body.ToArray())}");
        if (deadLetter.BasicProperties.Headers?.TryGetValue("x-death", out var death) == true)
            Console.WriteLine($"x-death metadata: {death}");
        channel.BasicAck(deadLetter.DeliveryTag, multiple: false);
    }
    else
    {
        Console.WriteLine("No DLQ message was read. Check the work consumer and queue bindings in Management UI.");
    }
}

static void TtlRedelivery(IModel channel)
{
    const string workExchange = "lab.retry.work";
    const string delayExchange = "lab.retry.delay";
    const string workQueue = "lab.retry.work-queue";
    const string delayQueue = "lab.retry.delay-5s";
    const string workKey = "work";
    const string delayKey = "after-5s";

    channel.ExchangeDeclare(workExchange, ExchangeType.Direct, durable: true);
    channel.ExchangeDeclare(delayExchange, ExchangeType.Direct, durable: true);

    channel.QueueDeclare(workQueue, durable: true, exclusive: false, autoDelete: false);
    channel.QueueBind(workQueue, workExchange, workKey);

    var delayArguments = new Dictionary<string, object>
    {
        ["x-message-ttl"] = 5000,
        ["x-dead-letter-exchange"] = workExchange,
        ["x-dead-letter-routing-key"] = workKey
    };

    channel.QueueDeclare(delayQueue, durable: true, exclusive: false, autoDelete: false, arguments: delayArguments);
    channel.QueueBind(delayQueue, delayExchange, delayKey);

    Publish(channel, workExchange, workKey, "Initial message: schedule one delayed retry");
    var delivery = channel.BasicGet(workQueue, autoAck: false);

    if (delivery is null)
    {
        Console.WriteLine("No message available from the work queue.");
        return;
    }

    var body = Encoding.UTF8.GetString(delivery.Body.ToArray());
    Console.WriteLine($"Received: {body}");

    // In a real consumer, only publish to the delay queue after a retryable failure.
    Publish(channel, delayExchange, delayKey, "Delayed retry message");
    channel.BasicAck(delivery.DeliveryTag, multiple: false);

    Console.WriteLine("Sent a copy to the 5-second TTL queue and acknowledged the original.");
    Console.WriteLine("Waiting for TTL expiry and dead-letter routing back to the work queue...");
    Thread.Sleep(TimeSpan.FromSeconds(6));

    var redelivered = channel.BasicGet(workQueue, autoAck: true);
    Console.WriteLine(redelivered is null
        ? "No delayed message found. Inspect topology and queue configuration."
        : $"Returned to work queue: {Encoding.UTF8.GetString(redelivered.Body.ToArray())}");
}

static void Bind(IModel channel, string exchange, string queue, string bindingKey)
{
    channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
    channel.QueueBind(queue, exchange, bindingKey);
}

static void Publish(
    IModel channel,
    string exchange,
    string routingKey,
    string message,
    IDictionary<string, object>? headers = null)
{
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    properties.ContentType = "text/plain";
    properties.Headers = headers is null ? null : new Dictionary<string, object>(headers);

    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchange, routingKey, mandatory: false, basicProperties: properties, body: body);
}


using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQHostService : IHostedService
{
    private readonly IRabbitMQConsumer _rabbitMQConsumer;

    public RabbitMQHostService(IRabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {

        ProductUpdateNameConsume();
        ProductDeleteConsume();

        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();
        return Task.CompletedTask;
    }

    public void ProductUpdateNameConsume()
    {
        string routingKey = "product.update.name";
        string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange")!;
        string queueName = "orders.product.update.name.queue";

        _rabbitMQConsumer.Consumer(routingKey, exchangeName, queueName);
    }
    public void ProductDeleteConsume()
    {
        string routingKey = "product.delete";
        string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange")!;
        string queueName = "orders.product.delete.queue";

        _rabbitMQConsumer.Consumer(routingKey, exchangeName, queueName);
    }


}

using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteHostService : IHostedService
{
    private readonly IRabbitMQConsumer _rabbitMQConsumer;

    public RabbitMQProductDeleteHostService(IRabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, object>()
        {
            {"x-match", "all"},
            {"event" , "product.delete"},
            {"RowCount" , 1},
        };
        string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange")!;
        string queueName = "orders.product.delete.queue";
        string eventName = "delete";

        _rabbitMQConsumer.Consumer(headers, exchangeName, queueName, eventName);

        return Task.CompletedTask;

    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();
        return Task.CompletedTask;
    }

}
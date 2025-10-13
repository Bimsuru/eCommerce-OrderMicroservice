
using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductUpdateHostService : IHostedService
{
    private readonly IRabbitMQConsumer _rabbitMQConsumer;

    public RabbitMQProductUpdateHostService(IRabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {

        var headers = new Dictionary<string, object>()
        {
            {"x-match", "all"},
            {"event" , "product.update"},
            {"RowCount" , 1},
        };


        string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange")!;
        string queueName = "orders.product.update.queue";
        string eventName = "update";

        _rabbitMQConsumer.Consumer(headers, exchangeName, queueName, eventName);

        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();
        return Task.CompletedTask;
    }
}


namespace BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQConsumer
{
    void Consumer(string routingKey, string exchangeName, string queueName);
    void Dispose();
}

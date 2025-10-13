
namespace BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQConsumer
{
    void Consumer(Dictionary<string, object> headers, string exchangeName, string queueName, string eventName);
    void Dispose();
}

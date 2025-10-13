using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;

        string hostName = _configuration["RABBITMQ_HOST"]!;
        string port = _configuration["RABBITMQ_PORT"]!;
        string userName = _configuration["RABBITMQ_USER_NAME"]!;
        string password = _configuration["RABBITMQ_PASSWORD"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            Port = Convert.ToInt32(port),
            UserName = userName,
            Password = password
        };

        // Create connection
        _connection = connectionFactory.CreateConnection();

        // create channel
        _channel = _connection.CreateModel();

    }

    public void Consumer(string routingKey, string exchangeName, string queueName)
    {
        // Exchange config
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        // exclusive mean for the false - can access this queue any connection
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        // arguments --> x-message-ttl | x-max-lenght | x-expired

        // Bind to the queue into this message using maching routingKey
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

        // now consume ready because rabbitmq client and server now connect with a channel
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        // Recieved messages from the queue
        consumer.Received += (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string? messageString = Encoding.UTF8.GetString(body);

            if (messageString != null && routingKey == "product.update.name")
            {
                var productUpdateMessage = JsonSerializer.Deserialize<ProductUpdateMessage>(messageString);
                _logger.LogInformation($"Product id : {productUpdateMessage!.ProductID} and product name : {productUpdateMessage.ProductName} are updated.");
            }
            else if(messageString != null && routingKey == "product.delete")
            {
                var productDeleteMessage = JsonSerializer.Deserialize<ProductDeleteMessage>(messageString);
                _logger.LogInformation($"Product id : {productDeleteMessage!.ProductID} and product name : {productDeleteMessage.ProductName} are deleted.");
            }

        };

        // After the consume messages and server know reciverd all the messsages by client
        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);


    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

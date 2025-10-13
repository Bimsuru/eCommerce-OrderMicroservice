namespace BusinessLogicLayer.RabbitMQ;

public record ProductDeleteMessage(
    Guid ProductID, string ProductName
);

namespace BusinessLogicLayer.RabbitMQ;

public record ProductUpdateMessage(
    Guid ProductID, string NewProductName
);

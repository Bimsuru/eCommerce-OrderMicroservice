namespace BusinessLogicLayer.DTO;

public record ProductResponse(
    Guid ProductID,
    string? ProductName,
    double UnitPrice,
    int QuantityInStock

);


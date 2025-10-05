
namespace BusinessLogicLayer.DTO;

public record OrderResponse(
    Guid OrderID,
    Guid UserID,
    string? Email,
    string? PersonName,
    DateTime OrderDate,
    decimal TotalBill,
    List<OrderItemResponse> OrderItems
)

{
    public OrderResponse() : this(default, default, default, default, default, default, default!) 
    {
        
    }

}

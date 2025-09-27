using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace APILayer.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAllOrders(Guid? productid, Guid? userid, DateTime? orderDate)
    {
        // Create filter variable
        FilterDefinition<Order> filter = Builders<Order>.Filter.Empty;

        if (productid.HasValue)
        {
            filter &= Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems, Builders<OrderItem>.Filter.Eq(temp => temp.ProductID, productid));
        }
        
        if (userid.HasValue)
        {
            filter = Builders<Order>.Filter.Eq(temp => temp.UserID, userid);
        }

        if (orderDate.HasValue)
        {
            var startDate = orderDate.Value.Date;
            var endDate = startDate.AddDays(1);

            filter &= Builders<Order>.Filter.Gte(s => s.OrderDate, startDate) & Builders<Order>.Filter.Lt(e => e.OrderDate, endDate);
        }

        // Invoke order service
        var ordersRes = await _orderService.GetOrdersByCondition(filter);

        if (ordersRes.Count == 0)
            return BadRequest(ordersRes);

        return Ok(ordersRes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        // Create filter with id 
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, id);

        var orderRes = await _orderService.GetOrderByCondition(filter);

        if (orderRes == null)
            return NotFound(orderRes);

        return Ok(orderRes);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> AddOrder(OrderAddRequest orderAddRequest)
    {
        var orderRes = await _orderService.AddOrder(orderAddRequest);

        if (orderRes == null)
            return Problem("Error in adding order");

        return Created($"api/v1/orders/{orderRes.OrderID}", orderRes);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderResponse>> UpdateOrder(OrderUpdateRequest orderUpdateRequest, Guid id)
    {
        if (orderUpdateRequest.OrderID != id)
        {
            return BadRequest("OrderID in the URL doesn't match with the OrderID in the Request body");
        }

        var orderRes = await _orderService.UpdateOrder(orderUpdateRequest);

        if (orderRes == null)
            return Problem("Error in updating order");

        return Ok(orderRes);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<OrderResponse>> DeleteOrder(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid order ID");
        }
        var isDeleted = await _orderService.DeleteOrder(id);

        if (!isDeleted)
        {
            return Problem("Error in deleting order");
        }

        return Ok($"Order Id : {id} is deleted");
    }
}

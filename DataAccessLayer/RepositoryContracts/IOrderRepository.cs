using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.RepositoryContracts;

public interface IOrderRepository
{
    Task<IEnumerable<Order?>> GetOrders();
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<Order?> AddOrder(Order order);
    Task<Order?> UpdateOrder(Order order);
    Task<bool> DeleteOrder(Guid orderid);
}

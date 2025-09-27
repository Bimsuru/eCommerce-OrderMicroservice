using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _collection;
    private readonly string _collectionName = "orders";

    public OrderRepository(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Order>(_collectionName);
    }
    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();

        await _collection.InsertOneAsync(order);

        return order;

    }

    public async Task<bool> DeleteOrder(Guid orderid)
    {
        // Create filter definition
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderid);

        DeleteResult result = await _collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;

    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = (await _collection.FindAsync(filter)).FirstOrDefault();
        return order;
    }

    public async Task<IEnumerable<Order?>> GetOrders()
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Empty;

        return (await _collection.FindAsync(filter)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        var orders = (await _collection.FindAsync(filter)).ToList();
        return orders;
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);

        // // check exitting order
        // var exittingOrder = (await _collection.FindAsync(filter)).FirstOrDefault();

        ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, order);

        if (result.IsModifiedCountAvailable == true && result.ModifiedCount > 0)
        {
            return order;
        }
        else
            return null;
    }
}

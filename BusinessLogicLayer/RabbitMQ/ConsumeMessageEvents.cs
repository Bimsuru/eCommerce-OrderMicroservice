
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.RabbitMQ;

public class ConsumeMessageEvents
{
    private ILogger<ConsumeMessageEvents> _logger;
    private IDistributedCache _distributedCache;
    public ConsumeMessageEvents(ILogger<ConsumeMessageEvents> logger, IDistributedCache distributedCache)
    {
        _logger = logger;
        _distributedCache = distributedCache;
    }
    public async Task ProductUpdateEvent(string message)
    {
        var productUpdateMessage = JsonSerializer.Deserialize<ProductResponse>(message);
        _logger.LogInformation($"Product id : {productUpdateMessage!.ProductID} and product name : {productUpdateMessage.ProductName} are updated.");

        string cacheKey = $"product:{productUpdateMessage.ProductID}";

        var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

        await _distributedCache.SetStringAsync(cacheKey, message, options);
    }

    public async Task ProductDeleteEvent(string message)
    {
        var productDeleteMessage = JsonSerializer.Deserialize<ProductDeleteMessage>(message);
        _logger.LogInformation($"Product id : {productDeleteMessage!.ProductID} and product name : {productDeleteMessage.ProductName} are deleted.");

        string cacheKey = $"product:{productDeleteMessage.ProductID}";

        await _distributedCache.RemoveAsync(cacheKey);
    }
}

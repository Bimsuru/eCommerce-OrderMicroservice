using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    public ProductMicroserviceClient(HttpClient httpClient, ILogger<ProductMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
    {
        try
        {
            // Check redis cache in this existing product 
            /* cache memory in product --> "Key": "product.id" 
                                           "Values": {"ProductName:..", ""}
            */
            string cacheKey = $"product:{id}";
            string? productCacheRes =  await _distributedCache.GetStringAsync(cacheKey);

            // Check product cache response is empty or not
            if (productCacheRes != null)
            {
                var cacheProduct = JsonSerializer.Deserialize<ProductResponse>(productCacheRes);
                return cacheProduct;
            }
    
            var response = await _httpClient.GetAsync($"/gateway/products/{id}");
    
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // avoid to cache memory in this fallback data
                    var productFromFallback = await response.Content.ReadFromJsonAsync<ProductResponse>();

                    if (productFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy not implemented.");
                    }

                    return productFromFallback;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with StatusCode {HttpStatusCode.InternalServerError}");
                }
            }

            var existingProduct = await response.Content.ReadFromJsonAsync<ProductResponse>();

            if (existingProduct == null)
                throw new ArgumentException("This product id is invalid");

       

            // convert product object serialize into json
            string productJson = JsonSerializer.Serialize(existingProduct);

            // The cache options for the entry(cache expiration time)
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                                                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));  // give 30s for the this key value product store
                                                            // .SetSlidingExpiration(TimeSpan.FromSeconds(100));  // but retrive 10s for this product if its not remove 10s not waiting 30s 

            // Write product object into cache memory
            await _distributedCache.SetStringAsync(cacheKey, productJson, options);

            return existingProduct;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation queue is full, now reject the number of requests now");

            return new ProductResponse(
                ProductID: Guid.Empty,
                ProductName: "Temporarily Unavailable (Bulkhead)",
                Category: "Temporarily Unavailable (Bulkhead)",
                UnitPrice: 0,
                QuantityInStock: 0
            );
        }

    }
}

using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductMicroserviceClient> _logger;
    public ProductMicroserviceClient(HttpClient httpClient, ILogger<ProductMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/products/{id}");
    
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
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

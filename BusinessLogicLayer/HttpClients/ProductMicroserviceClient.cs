using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    private readonly HttpClient _httpClient;
    public ProductMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
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
}

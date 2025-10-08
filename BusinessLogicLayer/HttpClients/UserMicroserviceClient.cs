using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BusinessLogicLayer.HttpClients;

public class UserMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserMicroserviceClient> _logger;
    public UserMicroserviceClient(HttpClient httpClient, ILogger<UserMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        try
        {
            HttpResponseMessage? response = await _httpClient.GetAsync($"/api/v1/users/{id}");

            // check response is success or not
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
                    //throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}");

                    // added Temporarily fault data
                    return new UserResponse(
                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        UserID: Guid.Empty,
                        Gender: "Temporarily Unavailable"
                    );

                }
            }

            var existinguser = await response.Content.ReadFromJsonAsync<UserResponse>();

            if (existinguser == null)
                throw new ArgumentException("Invalid user id");

            return existinguser;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "After the request failures, circuit breacker is now opern state, but not successfull retrun the dummy data");

            return new UserResponse(
                PersonName: "Temporarily Unavailable (circuit breacker)",
                Email: "Temporarily Unavailable (circuit breacker)",
                UserID: Guid.Empty,
                Gender: "Temporarily Unavailable (circuit breacker)"
            );
        }

        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Dependency service get more time to response");

            return new UserResponse(
                PersonName: "Temporarily Unavailable (timeout)",
                Email: "Temporarily Unavailable (timeout)",
                UserID: Guid.Empty,
                Gender: "Temporarily Unavailable (timeout)"
            );
        }
    }
}

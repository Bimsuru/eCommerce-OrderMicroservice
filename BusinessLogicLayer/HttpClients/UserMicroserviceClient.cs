using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.HttpClients;

public class UserMicroserviceClient
{
    private readonly HttpClient _httpClient;
    public UserMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
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
}

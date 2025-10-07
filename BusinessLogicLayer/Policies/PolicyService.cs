
using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies;

public class PolicyService : IPolicyService
{
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(ILogger<PolicyService> logger)
    {
        _logger = logger;
    }
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int count, int time)
    {
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
            !r.IsSuccessStatusCode).WaitAndRetryAsync(
                retryCount: count, // Number of retries
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(time, retryAttempt)), // Delay between retries
                onRetry: (outcome, timespan, retryattempt, context) =>
                {
                    _logger.LogInformation($"Retry {retryattempt} after {timespan.TotalSeconds} seconds");
                }
            );

        return retryPolicy;
    }
}

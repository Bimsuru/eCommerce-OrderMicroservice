
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;


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

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int openReqLimit, int breakTime)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
        !r.IsSuccessStatusCode).CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: openReqLimit,
            durationOfBreak: TimeSpan.FromMinutes(breakTime),
            onBreak: (outcome, timespan) =>
            {
                _logger.LogInformation($"After {openReqLimit} requests failures, now  circuit breaker open state within {timespan.TotalMinutes} minutes and all the requests are block this time");
            },

            onReset: () =>
            {
                _logger.LogInformation($"Now circuit breaker half opern state after the total time of breaking time and now one request allow to send dependency service");
            }
        );

        return circuitBreakerPolicy;
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;


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

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int openReqLimit, TimeSpan breakTime)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
        !r.IsSuccessStatusCode).CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: openReqLimit,
            durationOfBreak: breakTime,
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

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r =>
        !r.IsSuccessStatusCode)
        .FallbackAsync(async (context) =>
        {
            _logger.LogWarning("Fallback policy triggerd, request failed and dummy data return");

            ProductResponse productResponse = new ProductResponse
            (
                ProductID: Guid.Empty,
                ProductName: "Temporarily Unavailable (fallback)",
                Category: "Temporarily Unavailable (fallback)",
                UnitPrice: 0,
                QuantityInStock: 0
            );

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(productResponse), Encoding.UTF8, "application/json")
            };

            return response;
        });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeOutPolicy(TimeSpan rangeOfTimeOut)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(rangeOfTimeOut);
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(int maxNumberOfRequests, int queueSizeofRequests)
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: maxNumberOfRequests,
            maxQueuingActions: queueSizeofRequests,
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("Maximum number of request queue is full now and block the requests into dependency service");

                throw new BulkheadRejectedException("Bulkhead queue is full");
            }
        );

        return policy;

    }

    public IAsyncPolicy<HttpResponseMessage> UserServiceCombinedPolicy()
    {
        var retryPolicy = GetRetryPolicy(5, 2);
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(1));
        var timeOutPolicy = GetTimeOutPolicy(TimeSpan.FromMilliseconds(1500));

        AsyncPolicyWrap<HttpResponseMessage> usersCombinedPolicies = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeOutPolicy);

        return usersCombinedPolicies;
    }
    public IAsyncPolicy<HttpResponseMessage> ProductServiceCombinedPolicy()
    {
        var fallbackPolicy = GetFallbackPolicy();
        var bulkheadPolicy = GetBulkheadPolicy(2, 40);

        AsyncPolicyWrap<HttpResponseMessage> productCombinedPolicies = Policy.WrapAsync(fallbackPolicy, bulkheadPolicy);

        return productCombinedPolicies;
    }
}

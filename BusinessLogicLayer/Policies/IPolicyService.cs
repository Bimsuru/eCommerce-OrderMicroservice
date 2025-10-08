using Polly;

namespace BusinessLogicLayer.Policies;

public interface IPolicyService
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int count, int time);
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int openReqLimit, TimeSpan breakTime);
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    IAsyncPolicy<HttpResponseMessage> GetTimeOutPolicy(TimeSpan rangeOfTimeOut);
    IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(int maxNumberOfRequests, int queueSizeofRequests);
    IAsyncPolicy<HttpResponseMessage> UserServiceCombinedPolicy();
    IAsyncPolicy<HttpResponseMessage> ProductServiceCombinedPolicy();
}

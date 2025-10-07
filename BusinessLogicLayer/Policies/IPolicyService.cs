using Polly;

namespace BusinessLogicLayer.Policies;

public interface IPolicyService
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int count, int time);
}

using DnsClient.Internal;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    public PollyPolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).
        WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retryAttempts => TimeSpan.FromSeconds(Math.Pow(2, retryAttempts)), onRetry: (outcome, timesspan, retryAttempt, context) =>
        {
            // To do: add logs
            _logger.LogInformation($"Retry {retryAttempt} after {timesspan.TotalSeconds} seconds");
        });
        return policy;
    }
    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).
             CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, durationOfBreak: durationOfBreak, onBreak: (outcome, timespan) =>
             {
                 _logger.LogInformation($"Circuit breaker opened for {timespan.TotalSeconds} seconds due to consecutive 3 failures. The subsequent requests will be blocked");
             }, onReset: () =>
             {
                 _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
             });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeOut)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeOut);
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy<T>(Func<T> fallBackDataFactory)
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode).
            FallbackAsync(async (context) =>
            {
                _logger.LogInformation("Fallback triggered: The request failed, returning dummy data");
                var fallBackData = fallBackDataFactory();                
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable) { Content = new StringContent(JsonSerializer.Serialize(fallBackData), Encoding.UTF8, "application/json") };
                return response;
            });
        return policy;
    }
    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization, int maxQueuingActions)
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: maxParallelization, // Allows up to concurrent requests
                maxQueuingActions: maxQueuingActions, //Queue up to 40 additional requests
                onBulkheadRejectedAsync: (context) =>
                {
                    _logger.LogWarning("BulkheadIsolation triggered, Can't send any more requets since the queue is full");
                    throw new BulkheadRejectedException("Bulkhead queue is full");
                }
                );
        return policy;
    }
     
}

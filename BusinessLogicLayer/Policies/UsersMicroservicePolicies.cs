using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).
        WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retryAttempts => TimeSpan.FromSeconds(Math.Pow(2,retryAttempts)), onRetry: (outcome, timesspan, retryAttempt, context) =>
        {
            // To do: add logs
            _logger.LogInformation($"Retry {retryAttempt} after {timesspan.TotalSeconds} seconds");
        });
        return policy;
    }
    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).
             CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromSeconds(30), onBreak: (outcome, timespan) => { 
                 _logger.LogInformation($"Circuit breaker opened for {timespan.TotalSeconds} seconds due to consecutive 3 failures. The subsequent requests will be blocked");
             }, onReset: () => {
                 _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
             });
        return policy;
    }

}

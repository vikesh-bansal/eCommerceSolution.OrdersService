using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Pipelines.Sockets.Unofficial.Arenas;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersMicroserviceClient> _logger;
        private readonly IDistributedCache _distributedCache;
        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public async Task<UserDTO?> GetUserByUserID(Guid userID)
        {
            UserDTO? user = null;
            try
            {
                string userCacheKey = $"UserId:{userID}";
                string? cacheUser = await _distributedCache.GetStringAsync(userCacheKey);
                if (cacheUser != null)
                {
                    user = JsonSerializer.Deserialize<UserDTO?>(cacheUser);
                }
                if (user == null)
                {
                    HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userID}");
                    if (!response.IsSuccessStatusCode)
                    {
                        if(response.StatusCode== System.Net.HttpStatusCode.ServiceUnavailable)
                        { 
                            user = await response.Content.ReadFromJsonAsync<UserDTO?>();
                            if (user != null)
                            {
                                string userJson = JsonSerializer.Serialize(user);
                                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300)).SetSlidingExpiration(TimeSpan.FromSeconds(100));
                                _distributedCache.SetString(userCacheKey, userJson, cacheOptions);
                            }
                        }
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            user = null;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                            return new UserDTO(PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable", Gender: "Temporarily Unavailable", UserID: Guid.Empty);  //Implementing Fault data in case of exception
                        }
                    }
                    else
                    {
                        user = await response.Content.ReadFromJsonAsync<UserDTO?>();
                    
                        string userJson = JsonSerializer.Serialize(user);
                        DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300)).SetSlidingExpiration(TimeSpan.FromSeconds(100));
                        _distributedCache.SetString(userCacheKey, userJson, cacheOptions);
                    }
                    if (user == null)
                    {
                        throw new ArgumentException("Invalid User ID");
                    }
                }
                return user;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");
                //Retun fault data in case of excetions when circuit breaker opened due repetitive failures
                return new UserDTO(PersonName: "Temporarily Unavailable (circuit breaker)", Email: "Temporarily Unavailable (circuit breaker)", Gender: "Temporarily Unavailable (circuit breaker)", UserID: Guid.Empty);  //Implementing Fault data in case of exception
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "Timeout occured while fetching user data. Returning dummy data.");
                return new UserDTO(PersonName: "Temporarily Unavailable (timeout)", Email: "Temporarily Unavailable (timeout)", Gender: "Temporarily Unavailable (timeout)", UserID: Guid.Empty);  //Implementing Fault data in case of time out exception
            }

        }
    }
}

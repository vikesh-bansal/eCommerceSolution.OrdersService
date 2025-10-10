using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _iLogger;
    private readonly IDistributedCache _distributedCache;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> iLogger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _iLogger = iLogger;
        _distributedCache = distributedCache;
    }
    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        ProductDTO? product = null;
        try
        {
            //Key: product: 123
            //Value: { ""ProductName: "..", ... }
            string cachKey = $"product:{productId}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cachKey);
            if (cachedProduct != null)
            {
                product = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
            }
            else
            {
                HttpResponseMessage _response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productId}");
                if (!_response.IsSuccessStatusCode)
                {
                    if (_response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        product = await _response.Content.ReadFromJsonAsync<ProductDTO?>();

                        if (product == null)
                        {
                            throw new NotImplementedException("Fallback policy was not implemented");
                        }
                    }

                    if (_response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        product = null;
                    }
                    else if (_response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                    {
                        throw new HttpRequestException("Bad Request", null, System.Net.HttpStatusCode.BadGateway);
                    }
                    else
                    {
                        throw new HttpRequestException($"Http request failed with status code {_response.StatusCode}");
                    }
                }
                else
                {
                    product = await _response.Content.ReadFromJsonAsync<ProductDTO?>();
                    if (product == null)
                    {
                        throw new ArgumentException("Invalid Product ID");
                    }

                    //Key: product: {productID}
                    //Value: { "ProductName": "..", .. }
                    string productJson = JsonSerializer.Serialize(product);
                    DistributedCacheEntryOptions dOption = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300)).SetSlidingExpiration(TimeSpan.FromSeconds(100));
                    string cacheKeytoWrite = $"product:{productId}";
                    _distributedCache.SetString(cacheKeytoWrite, productJson, dOption);

                }
            }

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _iLogger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");
            return new ProductDTO(ProductID: Guid.NewGuid(), ProductName: "Temporarily Unavailable (Bulkhead)", Category: "Temporarily Unavaliable (Bulkhead)", UnitPrice: 0, QuantityInStock: 0);
        }

    }
}
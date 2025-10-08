using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _iLogger;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> iLogger)
    {
        _httpClient = httpClient;
        _iLogger = iLogger;
    }
    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            HttpResponseMessage _response = await _httpClient.GetAsync($"/api/products/search/product-id/{productId}");
            if (!_response.IsSuccessStatusCode)
            {
                if (_response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
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
                ProductDTO? product = await _response.Content.ReadFromJsonAsync<ProductDTO?>();
                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }
                return product;
            }
        }
        catch (BulkheadRejectedException ex)
        {
            _iLogger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");
            return new ProductDTO(ProductID: Guid.NewGuid(), ProductName: "Temporarily Unavailable (Bulkhead)",Category:"Temporarily Unavaliable (Bulkhead)", UnitPrice:0,QuantityInStock:0);
        }

    }
}
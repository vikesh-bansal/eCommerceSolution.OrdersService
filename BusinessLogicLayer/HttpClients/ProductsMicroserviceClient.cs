using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
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
}
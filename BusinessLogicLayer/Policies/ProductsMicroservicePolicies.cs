using DnsClient.Internal;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;
    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode).
            FallbackAsync(async (context) =>
            {
                _logger.LogInformation("Fallback triggered: The request failed, returning dummy data");
                ProductDTO product = new ProductDTO(ProductID: Guid.Empty, ProductName: "Temporarily Unavailable (fallback)", Category: "Temporarily Unavailable (fallback)", UnitPrice: 0, QuantityInStock: 0);
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json") };
                return response;
            });
        return policy;
    }
}

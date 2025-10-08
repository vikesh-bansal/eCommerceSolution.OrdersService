using DnsClient.Internal;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly IPollyPolicies _pollyPolicies;
    private readonly ILogger<ProductsMicroservicePolicies> _logger;
    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }  

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var fallBackPolicy = _pollyPolicies.GetFallbackPolicy<ProductDTO>(() => new ProductDTO(ProductID: Guid.Empty, ProductName: "Temporarily Unavailable (fallback)", Category: "Temporarily Unavailable (fallback)", UnitPrice: 0, QuantityInStock: 0));
        var bulkHeadIsolation = _pollyPolicies.GetBulkheadIsolationPolicy(1, 2);
        AsyncPolicyWrap<HttpResponseMessage> policy = Policy.WrapAsync(fallBackPolicy, bulkHeadIsolation);
        return policy;

    }
}

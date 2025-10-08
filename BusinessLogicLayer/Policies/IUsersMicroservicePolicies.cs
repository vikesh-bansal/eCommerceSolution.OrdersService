﻿
using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
}

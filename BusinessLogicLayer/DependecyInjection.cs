using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependecyInjection
{
    public static IServiceCollection AddDomainLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // To do: Add domain logic layer services into the Ioc container

        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();// load all validators assembly
        services.AddAutoMapper(cfg=> { },typeof(OrderToOrderResponseMappingProfile).Assembly); // load all mapping profile assembly
        services.AddScoped<IOrdersService, OrderService>();
        return services;
    }
}

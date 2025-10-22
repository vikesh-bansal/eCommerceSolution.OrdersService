using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
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
        services.AddAutoMapper(cfg => { }, typeof(OrderToOrderResponseMappingProfile).Assembly); // load all mapping profile assembly
        services.AddScoped<IOrdersService, OrderService>();
        services.AddStackExchangeRedisCache(options => { options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}"; });
     
        services.AddSingleton<IRabbitMQConnectionProvider, RabbitMQConnectionProvider>();
        services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();
        services.AddHostedService<RabbitMQProductNameUpdateHostedService>();

        services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();
        services.AddHostedService<RabbitMQProductDeletionHostedService>();

        return services;
    }
}

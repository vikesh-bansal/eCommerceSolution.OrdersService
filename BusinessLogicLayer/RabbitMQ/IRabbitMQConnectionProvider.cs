using RabbitMQ.Client; 

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
public interface IRabbitMQConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
    Task<IChannel> GetChannelAsync();
}
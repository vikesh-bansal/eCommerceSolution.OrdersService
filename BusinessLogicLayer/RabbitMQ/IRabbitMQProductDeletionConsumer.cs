using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{

    Task Consume();
    void Dispose();
}
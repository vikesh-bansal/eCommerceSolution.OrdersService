using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionConsumer : IRabbitMQProductDeletionConsumer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;
    private readonly IRabbitMQConnectionProvider _rabbitMQConnectionProvider;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    public RabbitMQProductDeletionConsumer(IConfiguration configuration, IRabbitMQConnectionProvider rabbitMQConnectionProvider, ILogger<RabbitMQProductDeletionConsumer> logger, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _configuration = configuration;
        _logger = logger;
        _rabbitMQConnectionProvider = rabbitMQConnectionProvider;
        _productsMicroserviceClient = productsMicroserviceClient;
    }

    public async Task Consume()
    {
        // string routingKey = "product.delete";
        //string routingKey = "product.#";//# could be single zero or multiple words like % in sql
        var headers = new Dictionary<string, object>() { { "x-match", "all" }, { "event", "product.delete" }, { "RowCount", 1 } };
        string queueName = "orders.product.delete.queue";
        IChannel _channel = await _rabbitMQConnectionProvider.GetChannelAsync();
        // create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        //await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);
        //await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
        await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //x=message-ttl | x-max-length | x-expired
                                                                                                                                 //Bind the message to exchange
                                                                                                                                 //await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);
        await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);
        AsyncEventingBasicConsumer eventingBasicConsumer = new AsyncEventingBasicConsumer(_channel);
        eventingBasicConsumer.ReceivedAsync += EventingBasicConsumer_ReceivedAsync;
        await _channel.BasicConsumeAsync(queue: queueName, consumer: eventingBasicConsumer, autoAck: true);
    }

    public void Dispose()
    {
    }

    private async Task EventingBasicConsumer_ReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        byte[] body = eventArgs.Body.ToArray();
        string message = Encoding.UTF8.GetString(body);
        ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);
        if (productDeletionMessage != null)
        {
            await _productsMicroserviceClient.RemoveProductCache(productDeletionMessage.ProductID);
            _logger.LogInformation($"Product deleted:{productDeletionMessage.ProductID}, Product Name: {productDeletionMessage.ProductName}");
        }
        else
        {
            _logger.LogInformation("Failed to deserialize ProductDeletionMessage");
        }
    }

}

using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
        private readonly IRabbitMQConnectionProvider _rabbitMQConnectionProvider;
        private readonly ProductsMicroserviceClient _productsMicroserviceClient;
        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, IRabbitMQConnectionProvider rabbitMQConnectionProvider, ILogger<RabbitMQProductNameUpdateConsumer> logger, ProductsMicroserviceClient productsMicroserviceClient)
        {
            _configuration = configuration;
            _logger = logger;
            _rabbitMQConnectionProvider = rabbitMQConnectionProvider;
            _productsMicroserviceClient=productsMicroserviceClient;

        }

        public async Task Consume()
        {
            // string routingKey = "product.update.name";
            Dictionary<string, object> headers = new Dictionary<string, object>() {
                { "x-match","all"},
                {"event", "product.update" }, 
                {"RowCount",1 }
            };
            string queueName = "orders.product.update.name.queue";
            IChannel _channel = await _rabbitMQConnectionProvider.GetChannelAsync();
            // create exchange
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
            //await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);
            // await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //x=message-ttl | x-max-length | x-expired
                                                                                                                                     //Bind the message to exchange
            //await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments:headers);
            AsyncEventingBasicConsumer eventingBasicConsumer = new AsyncEventingBasicConsumer(_channel);
            eventingBasicConsumer.ReceivedAsync += EventingBasicConsumer_ReceivedAsync;
            await _channel.BasicConsumeAsync(queue: queueName, consumer: eventingBasicConsumer, autoAck: true);
        }

        public void Dispose()
        {
        }

        private Task EventingBasicConsumer_ReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            //  ProductNameUpdateMessage? productNameUpdateMessage = JsonSerializer.Deserialize<ProductNameUpdateMessage>(message);
            ProductDTO? productNameUpdateMessage = JsonSerializer.Deserialize<ProductDTO>(message);
            if (productNameUpdateMessage != null)
            {
                _productsMicroserviceClient.CreateProductCache(productNameUpdateMessage.ProductID, productNameUpdateMessage);
                //_logger.LogInformation($"Product name updated:{productNameUpdateMessage.ProductID}, New name: {productNameUpdateMessage.NewName}");
                _logger.LogInformation($"Product name updated:{productNameUpdateMessage.ProductID}, New name: {productNameUpdateMessage.ProductName}");
            }
            else
            {
                _logger.LogInformation("Failed to deserialize ProductNameUpdateMessage");
            }
            return Task.CompletedTask;
        }

    }
}

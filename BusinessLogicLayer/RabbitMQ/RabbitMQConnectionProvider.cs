
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQConnectionProvider : IRabbitMQConnectionProvider, IDisposable
{
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;
    public RabbitMQConnectionProvider(IConfiguration config)
    {
        _config = config;
    }
    public async Task<IChannel> GetChannelAsync()
    {
        if (_channel != null)
            return _channel;


        var connection = await GetConnectionAsync();
        _channel = await connection.CreateChannelAsync();
        return _channel;
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection != null)
        {
            return _connection;
        }
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ_HostName"],
            UserName = _config["RabbitMQ_UserName"],
            Password = _config["RabbitMQ_Password"],
            Port = int.Parse(_config["RabbitMQ_Port"]!),
        };
        _connection = await factory.CreateConnectionAsync();
        return _connection;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;
    }
}

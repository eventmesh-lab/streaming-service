using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using streaming_service.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace streaming_service.Infrastructure.Messaging
{
    public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQMessagePublisher(IConfiguration configuration)
        {
            var factory = new ConnectionFactory() 
            { 
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Ensure exchange exists
            _channel.ExchangeDeclare(exchange: "streaming-exchange", type: ExchangeType.Topic);
        }

        public Task PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: properties,
                                 body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}

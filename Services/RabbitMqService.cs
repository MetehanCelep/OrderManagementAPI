using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderManagementAPI.Services
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private IConnection? _connection;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                Port = configuration.GetValue<int>("RabbitMQ:Port"),
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            try
            {
                _connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                _connection = null; // işaretle
            }
        }

        public void SendMessage(string queueName, object message)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ connection is not available.");
            }

            try
            {
                using var channel = _connection.CreateModel(); // channel per call (thread-safe)
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

                _logger.LogInformation("Message sent to queue {Queue}: {Payload}", queueName, messageJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to queue {Queue}", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection.IsOpen) _connection.Close();
                    _connection.Dispose();
                }
            }
            catch { /* swallow dispose errors */ }
        }
    }
}

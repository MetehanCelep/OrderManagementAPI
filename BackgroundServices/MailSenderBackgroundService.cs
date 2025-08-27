using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderManagementAPI.BackgroundServices
{
    public class MailSenderBackgroundService : BackgroundService
    {
        private readonly ILogger<MailSenderBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public MailSenderBackgroundService(ILogger<MailSenderBackgroundService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    Port = _configuration.GetValue<int>("RabbitMQ:Port"),
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"]
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: "SendMail", durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("MailSenderBackgroundService RabbitMQ connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ for MailSenderBackgroundService");
                _connection = null;
                _channel = null;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel is not available, MailSenderBackgroundService will not start.");
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received mail request: {Message}", message);

                    var emailData = JsonSerializer.Deserialize<JsonElement>(message);
                    await SendEmailAsync(emailData, stoppingToken);

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Email sent successfully and message acknowledged");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing mail message");

                    // Retry limit: 3 kez denedikten sonra requeue etme
                    var redelivered = ea.Redelivered;
                    if (redelivered)
                    {
                        _logger.LogWarning("Message {Tag} already redelivered, rejecting permanently.", ea.DeliveryTag);
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    else
                    {
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            _channel.BasicConsume(queue: "SendMail", autoAck: false, consumer: consumer);

            _logger.LogInformation("MailSenderBackgroundService started listening for messages");

            return Task.CompletedTask;
        }

        private async Task SendEmailAsync(JsonElement emailData, CancellationToken ct)
        {
            try
            {
                await Task.Delay(2000, ct);

                var email = emailData.GetProperty("Email").GetString();
                var customerName = emailData.GetProperty("CustomerName").GetString();
                var orderId = emailData.GetProperty("OrderId").GetInt32();
                var totalAmount = emailData.GetProperty("TotalAmount").GetDecimal();

                _logger.LogInformation("EMAIL SENT - To: {Email}, Customer: {Customer}, OrderId: {OrderId}, Amount: {Amount:C}",
                    email, customerName, orderId, totalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw;
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}

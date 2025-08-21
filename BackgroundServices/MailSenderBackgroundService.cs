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
        private IConnection _connection;
        private IModel _channel;

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
                var factory = new ConnectionFactory()
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
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation($"Received mail request: {message}");

                        // Simulate email sending
                        var emailData = JsonSerializer.Deserialize<JsonElement>(message);
                        await SendEmailAsync(emailData);

                        // Acknowledge the message
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        _logger.LogInformation("Email sent successfully and message acknowledged");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing mail message");
                        // Reject the message and requeue it
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(queue: "SendMail", autoAck: false, consumer: consumer);

                _logger.LogInformation("MailSenderBackgroundService started listening for messages");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MailSenderBackgroundService ExecuteAsync");
                throw;
            }
        }

        private async Task SendEmailAsync(JsonElement emailData)
        {
            try
            {
                // Simulate email sending delay
                await Task.Delay(2000);

                var email = emailData.GetProperty("Email").GetString();
                var customerName = emailData.GetProperty("CustomerName").GetString();
                var orderId = emailData.GetProperty("OrderId").GetInt32();
                var totalAmount = emailData.GetProperty("TotalAmount").GetDecimal();

                // Here you would integrate with a real email service (SendGrid, SMTP, etc.)
                _logger.LogInformation($"EMAIL SENT - To: {email}, Customer: {customerName}, OrderId: {orderId}, Amount: {totalAmount:C}");
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
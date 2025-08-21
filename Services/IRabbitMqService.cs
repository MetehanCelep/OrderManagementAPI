namespace OrderManagementAPI.Services
{
    public interface IRabbitMqService
    {
        void SendMessage(string queueName, object message);
    }
}
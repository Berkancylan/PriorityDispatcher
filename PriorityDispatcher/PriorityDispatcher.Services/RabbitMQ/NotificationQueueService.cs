using RabbitMQ.Client;
using System.Text;
using System.Text.Json; 
using PriorityDispatcher.Contracts.Models;
using PriorityDispatcher.Contracts.Interfaces;

namespace PriorityDispatcher.Services.RabbitMQ
{
    public class NotificationQueueService : INotificationQueueService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName = "priority_queue";

        public NotificationQueueService()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public async Task EnqueueAsync(NotificationTask notificationTask)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var arguments = new Dictionary<string, object>
            {
                {"x-max-priority", 3 }
            };

            await channel.QueueDeclareAsync(queue: _queueName,
                                           durable: false,
                                           exclusive: false,
                                           autoDelete: false,
                                           arguments: arguments!);

            var message = JsonSerializer.Serialize(notificationTask);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = false,
                Priority = (byte)notificationTask.Priority
            };

            await channel.BasicPublishAsync(exchange: "",
                                           routingKey: _queueName,
                                           mandatory: false,
                                           basicProperties: properties,
                                           body: body);

            Console.WriteLine($" [x] RabbitMQ v7 ile gönderildi: {notificationTask.Id}");
        }
        public int Count => 0; 
    }
}
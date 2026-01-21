using Microsoft.Extensions.Hosting;
using PriorityDispatcher.Contracts.Interfaces;
using PriorityDispatcher.Contracts.Models;
using PriorityDispatcher.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PriorityDispatcher.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IEncryptionService? _encryptionService;
        public Worker(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var arguments = new Dictionary<string, object> { { "x-max-priority", 3 } };
            await channel.QueueDeclareAsync(queue: "priority_queue", durable: false, exclusive: false, autoDelete: false, arguments: arguments!);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 3, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var task = JsonSerializer.Deserialize<NotificationTask>(message);

                    if (task != null && !string.IsNullOrEmpty(task.Content))
                    {
                        string password = _encryptionService!.Decrypt(task.Content);
                        await Task.Delay(1000);

                        Console.WriteLine($"[RabbitMQ-Worker] Ýþlendi: {task.Id} - Þifre: {password} - Öncelik: {ea.BasicProperties.Priority}");
                    }
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata oluþtu: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: "priority_queue", autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

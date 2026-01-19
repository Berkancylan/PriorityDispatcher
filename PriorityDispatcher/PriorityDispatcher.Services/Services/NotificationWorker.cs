using Microsoft.Extensions.Hosting;
using PriorityDispatcher.Contracts.Interfaces;
using PriorityDispatcher.Contracts.Models;
using PriorityDispatcher.Services;

namespace PriorityDispatcher.Services.Services
{
    public class NotificationWorker : BackgroundService, INotificationWorker
    {
        private readonly SemaphoreSlim _generalSemaphore = new SemaphoreSlim(3);

        private readonly INotificationQueueService? _notificationQueueService;
        private readonly IEncryptionService? _encryptionService;
        public NotificationWorker(INotificationQueueService queueService, IEncryptionService encryptionService)
        {
            _notificationQueueService = queueService;
            _encryptionService = encryptionService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _generalSemaphore.WaitAsync();

                NotificationTask? task;
                bool _queue = _notificationQueueService!.Dequeue(out task);

                if (_queue == true && !string.IsNullOrEmpty(task!.Content))
                {
                    var currentTask = task;
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            string password = _encryptionService!.Decrypt(currentTask.Content);
                            Thread.Sleep(1000);
                            Console.WriteLine($"[Thread-{Environment.CurrentManagedThreadId}] {currentTask.Id}:{password}");
                        }
                        finally
                        {
                            _generalSemaphore.Release();
                        }
                    }, stoppingToken);
                }
                else { _generalSemaphore.Release(); await Task.Delay(1000, stoppingToken); }
            }
        }
    }
}

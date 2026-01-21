using PriorityDispatcher.Contracts.Models;

namespace PriorityDispatcher.Contracts.Interfaces
{
    public interface INotificationQueueService
    {
        Task EnqueueAsync(NotificationTask notificationTask);
        int Count { get; }
    }
}
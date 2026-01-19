using Microsoft.AspNetCore.Mvc;
using PriorityDispatcher.Contracts.Interfaces;
using PriorityDispatcher.Contracts.Models;

namespace PriorityDispatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IEncryptionService _encryptionService;
        private readonly INotificationQueueService _notificationQueue;

        public NotificationController(IEncryptionService encryptionService, INotificationQueueService notificationQueue)
        {
            _encryptionService = encryptionService;
            _notificationQueue = notificationQueue;
        }

        [HttpPost("send")]
        public IActionResult SendNotification([FromBody] NotificationTask task)
        {
            task.Content = _encryptionService.Encryption(task.Content);

            _notificationQueue.Enqueue(task);

            return Ok(new { Message = "Kuyruğa alındı", TaskId = task.Id, DecryptMessage = task.Content });
        }

        [HttpPost("multiple-send")]
        public IActionResult SendBulk([FromBody] List<NotificationTask> models)
        {
            foreach (var model in models)
            {
                var task = new NotificationTask
                {
                    Id = Guid.NewGuid(),
                    Content = _encryptionService.Encryption(model.Content),
                    Priority = model.Priority
                };
                _notificationQueue.Enqueue(task);
            }
            return Ok($"{models.Count} adet mesaj kuyruğa eklendi.");
        }

        [HttpGet("count")]
        public IActionResult GetQueueCount()
        {
            var count = _notificationQueue.Count;
            return Ok(new { QueueLength = count });
        }
    }
}

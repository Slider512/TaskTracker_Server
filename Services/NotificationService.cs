using Microsoft.AspNetCore.SignalR;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTaskCreated(string taskId, string taskTitle)
        {
            await _hubContext.Clients.All.SendAsync("TaskCreated", new { taskId, taskTitle });
        }

        public async Task NotifyTaskUpdated(string taskId, string taskTitle)
        {
            await _hubContext.Clients.All.SendAsync("TaskUpdated", new { taskId, taskTitle });
        }

        public async Task NotifyTaskDeleted(string taskId)
        {
            await _hubContext.Clients.All.SendAsync("TaskDeleted", taskId);
        }
    }

    public class NotificationHub : Hub
    {
    }
}

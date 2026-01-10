using KanbanModel.ModelClasses;
using Microsoft.AspNetCore.SignalR;

namespace KanbanRestService.Hubs
{
    /// <summary>
    /// Create as an adapter to send task notifications via SignalR
    /// And to provide easier mocking in unit tests
    /// </summary>
    public class SignalRTaskNotification : ITaskNotifications
    {
        private readonly IHubContext<TasksHub> _tasksHubContext;

        public SignalRTaskNotification(IHubContext<TasksHub> tasksHubContext)
        {
            _tasksHubContext = tasksHubContext;            
        }
        public Task TaskCreated(KanbanTask task, CancellationToken ct)
        {
            return _tasksHubContext.Clients.All.SendAsync("TaskCreated", task, ct);
        }

        public Task TaskDeleted(int taskId, CancellationToken ct)
        {
            return _tasksHubContext.Clients.All.SendAsync("TaskDeleted", taskId, ct);
        }

        public Task TaskUpdated(KanbanTask task, CancellationToken ct)
        {
            return _tasksHubContext.Clients.All.SendAsync("TaskUpdated", task, ct);   
        }
    }
}

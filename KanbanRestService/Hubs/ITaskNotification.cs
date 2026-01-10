using KanbanModel.ModelClasses;

namespace KanbanRestService.Hubs
{
    public interface ITaskNotifications
    {
        Task TaskCreated(KanbanTask task, CancellationToken ct);
        Task TaskUpdated(KanbanTask task, CancellationToken ct);
        Task TaskDeleted(int taskId, CancellationToken ct);
    }
}

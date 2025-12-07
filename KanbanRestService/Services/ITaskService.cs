using KanbanModel.ModelClasses;

namespace KanbanRestService.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<KanbanTask>> GetAllTasksAsync();
        Task<KanbanTask?> GetTaskByIdAsync(int id);
        Task<KanbanTask> CreateTaskAsync(KanbanTask task);
        Task<bool> PartialUpdateTaskAsync(int id, KanbanTask task);
        Task<bool> UpdateTaskAsync(int id, KanbanTask task);
        Task<bool> DeleteTaskAsync(int id);

    }
}

using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using System.Threading;

namespace KanbanRestService.Services
{
    public interface ITaskService
    {
        Task<List<KanbanTask>> GetPaginatedTasksAsync(CancellationToken cancellationToken, string? status, int page = 0, int size = 10, List<string>? sort = null);
        Task<KanbanTask?> GetTaskByIdAsync(int id, CancellationToken cancellationToken);
        Task<KanbanTask?> CreateTaskAsync(CreateKanbanTaskRequest task, CancellationToken cancellationToken);
        Task<bool> PartialUpdateTaskAsync(int id, PartialUpdateKanbanTaskRequest task, CancellationToken cancellationToken);
        Task<bool> UpdateTaskAsync(int id, FullUpdateKanbanTaskRequest task, CancellationToken cancellationToken);
        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken);

    }
}

using KanbanModel.DTOs;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Services
{
    public interface ITaskService
    {
        Task<List<KanbanTask>> GetPaginatedTasksAsync(string? status, int page = 0, int size = 10, List<string>? sort = null);
        Task<KanbanTask?> GetTaskByIdAsync(int id);
        Task<KanbanTask?> CreateTaskAsync(CreateKanbanTaskRequest task);
        Task<bool> PartialUpdateTaskAsync(int id, PartialUpdateKanbanTaskRequest task);
        Task<bool> UpdateTaskAsync(int id, FullUpdateKanbanTaskRequest task);
        Task<bool> DeleteTaskAsync(int id);

    }
}

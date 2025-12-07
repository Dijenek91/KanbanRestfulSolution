using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.ModelClasses;

namespace KanbanRestService.Services
{
    public class TaskServiceHost : ITaskService
    {
        private IUnitOfWork<KanbanAppDbContext> _unitOfWork;
        private IGenericRepository<KanbanTask> _taskRepo;
        public TaskServiceHost(IUnitOfWork<KanbanAppDbContext> unitOfWork, IGenericRepository<KanbanTask> taskRepo)
        {
            _unitOfWork = unitOfWork;
            _taskRepo = taskRepo;
        }

        public async Task<KanbanTask> CreateTaskAsync(KanbanTask task)
        {
            _taskRepo.Add(task);
            await _unitOfWork.SaveAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var foundTask = await _taskRepo.FindAsync(id);
            if (foundTask == null)
            {
                return false;
            }
            _taskRepo.Delete(foundTask);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<KanbanTask>> GetAllTasksAsync()
        {
            return _taskRepo.GetAllRecordsAsync().Result.ToList();
        }

        public async Task<KanbanTask?> GetTaskByIdAsync(int id)
        {
            return await _taskRepo.FindAsync(id);
        }

        public async Task<bool> PartialUpdateTaskAsync(int id, KanbanTask task)
        {
            var existingTask = await _taskRepo.FindAsync(id);
            if (existingTask == null)
                return false;

            // Update only properties that were sent (NOT NULL)
            if (task.Name != null)
                existingTask.Name = task.Name;

            if (task.Description != null)
                existingTask.Description = task.Description;

            if (task.Status.HasValue)
                existingTask.Status = task.Status;

            if (task.Size != null)
                existingTask.Size = task.Size;

            if (task.PriorityEnum.HasValue)
                existingTask.PriorityEnum = task.PriorityEnum;

            _taskRepo.Update(existingTask);

            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, KanbanTask task)
        {
            if (task.Id != 0 && task.Id != id)
                throw new ArgumentException("ID in the body does not match ID in the URL.");

            var foundTask = await _taskRepo.FindAsync(id);
            if (foundTask == null)
            {
                return false;
            }

            foundTask.Name = task.Name;
            foundTask.Description = task.Description;
            foundTask.Status = task.Status;
            foundTask.Size = task.Size;
            foundTask.PriorityEnum = task.PriorityEnum;

            _taskRepo.Update(foundTask);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}

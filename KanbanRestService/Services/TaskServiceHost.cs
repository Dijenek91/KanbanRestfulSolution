using AutoMapper;
using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KanbanRestService.Services
{
    public class TaskServiceHost : ITaskService
    {
        private IUnitOfWork<KanbanAppDbContext> _unitOfWork;
        private IGenericRepository<KanbanTask> _taskRepo;
        private readonly IHubContext<TasksHub> _tasksHubContext;
        private readonly IMapper _mapper;

        public TaskServiceHost(IUnitOfWork<KanbanAppDbContext> unitOfWork, 
            IGenericRepository<KanbanTask> taskRepo, 
            IHubContext<TasksHub> tasksHubContext,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _taskRepo = taskRepo;
            _tasksHubContext = tasksHubContext;
            _mapper = mapper;
        }

        public async Task<KanbanTask> CreateTaskAsync(CreateKanbanTaskRequest createdTask, CancellationToken cancellationToken)
        {
            var mappedKanbanTask = _mapper.Map<KanbanTask>(createdTask);
            _taskRepo.Add(mappedKanbanTask);
            
            await _unitOfWork.SaveAsync(cancellationToken);
            await _tasksHubContext.Clients.All.SendAsync("TaskCreated", mappedKanbanTask, cancellationToken);
            
            return mappedKanbanTask;
        }

        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken)
        {
            if (id == 0)
                throw new ArgumentException("ID with 0 doesn't exist.");

            var foundTask = await _taskRepo.FindAsync(id, cancellationToken);
            if (foundTask == null)
            {
                return false;
            }
                        
            _taskRepo.Delete(foundTask);
            await _unitOfWork.SaveAsync(cancellationToken);

            
            await _tasksHubContext.Clients.All.SendAsync("TaskDeleted", foundTask.Id, cancellationToken);

            return true;
        }

        public async Task<List<KanbanTask>> GetPaginatedTasksAsync(CancellationToken cancellationToken,
            string? status,
            int page,
            int size,
            List<string>? sortFields)
        {
            var query = _taskRepo.GetQueryableEntities().AsNoTracking();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusEnum>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
            
            //Sorting
            query = ApplySorting(query, sortFields);

            //Pagination
            var total = await query.CountAsync<KanbanTask>(cancellationToken);
            var items = await query.Skip(page * size).Take(size).ToListAsync(cancellationToken);

            return items;
        }

        public async Task<KanbanTask?> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _taskRepo.FindAsync(id, cancellationToken);
        }

        public async Task<bool> PartialUpdateTaskAsync(int id, PartialUpdateKanbanTaskRequest taskRequest, CancellationToken cancellationToken)
        {
            var foundTask = await _taskRepo.FindAsync(id, cancellationToken);
            if (foundTask == null)
                return false;

            // Update only properties that were sent (NOT NULL)
            _mapper.Map(taskRequest, foundTask);   

            _taskRepo.Update(foundTask);

            await _unitOfWork.SaveAsync(cancellationToken);

            await _tasksHubContext.Clients.All.SendAsync("TaskUpdated", foundTask);

            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, FullUpdateKanbanTaskRequest taskRequest, CancellationToken cancellationToken)
        {
            if (id == 0)
                throw new ArgumentException("ID with 0 doesn't exist.");


            var foundTask = await _taskRepo.FindAsync(id, cancellationToken);
            if (foundTask == null)
            {
                return false;
            }

            _mapper.Map(taskRequest, foundTask);

            _taskRepo.Update(foundTask);
            await _unitOfWork.SaveAsync(cancellationToken);

            await _tasksHubContext.Clients.All.SendAsync("TaskUpdated", foundTask);

            return true;
        }


        #region
        private IQueryable<KanbanTask> ApplySorting(IQueryable<KanbanTask> query, List<string>? sortFields)
        {
            if (sortFields == null || sortFields.Count == 0)
                return query.OrderBy(t => t.Id); // sort by Id by default

            IOrderedQueryable<KanbanTask>? orderedQuery = null;

            foreach (var sort in sortFields)
            {
                var parts = sort.Split(',');
                var field = parts[0].Trim();
                var direction = (parts.Length > 1 ? parts[1] : "asc").Trim().ToLower();

                orderedQuery = ApplySingleSort(orderedQuery ?? query, field, direction);
            }

            return orderedQuery ?? query;
        }

        private IOrderedQueryable<KanbanTask> ApplySingleSort(IQueryable<KanbanTask> query, string field, string direction)
        {
            return (field, direction) switch
            {
                ("name", "asc") => query.OrderBy(t => t.Name),
                ("name", "desc") => query.OrderByDescending(t => t.Name),

                ("priority", "asc") => query.OrderBy(t => t.PriorityEnum),
                ("priority", "desc") => query.OrderByDescending(t => t.PriorityEnum),

                ("status", "asc") => query.OrderBy(t => t.Status),
                ("status", "desc") => query.OrderByDescending(t => t.Status),

                ("size", "asc") => query.OrderBy(t => t.Size),
                ("size", "desc") => query.OrderByDescending(t => t.Size),

                _ => query.OrderBy(t => t.Id)
            };
        }
        #endregion
    }
}

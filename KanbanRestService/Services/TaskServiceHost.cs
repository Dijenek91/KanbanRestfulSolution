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
        private readonly IUnitOfWork<KanbanAppDbContext> _unitOfWork;
        private readonly IGenericRepository<KanbanTask> _taskRepo;
        private readonly ITaskNotifications _notifications;
        private readonly IMapper _mapper;

        public TaskServiceHost(
            IUnitOfWork<KanbanAppDbContext> unitOfWork, 
            IGenericRepository<KanbanTask> taskRepo,
            ITaskNotifications notifications,
            IMapper mapper)
        {
            if(unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork), "[TaskServiceHost] Unit of work cannot be null.");
            if (taskRepo == null)
                throw new ArgumentNullException(nameof(taskRepo), "[TaskServiceHost] Task repository cannot be null.");
            if (notifications == null)
                throw new ArgumentNullException(nameof(notifications), "[TaskServiceHost] Task notification object cannot be null.");
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper), "[TaskServiceHost] Unit of work cannot be null.");

            _unitOfWork = unitOfWork;
            _taskRepo = taskRepo;
            _notifications = notifications;
            _mapper = mapper;
        }

        public async Task<KanbanTask> CreateTaskAsync(CreateKanbanTaskRequest createdTask, CancellationToken cancellationToken)
        {
            if (createdTask == null)
            {
                throw new ArgumentNullException(nameof(createdTask), "[CreateTaskAsync] Created task cannot be null.");
            }

            var mappedKanbanTask = _mapper.Map<KanbanTask>(createdTask);
            if (mappedKanbanTask == null)
            {
                throw new InvalidOperationException("[CreateTaskAsync] Mapping produced null KanbanTask.");
            }
            _taskRepo.Add(mappedKanbanTask);
            
            await _unitOfWork.SaveAsync(cancellationToken);
            await _notifications.TaskCreated(mappedKanbanTask, cancellationToken);

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

            
            await _notifications.TaskDeleted(foundTask.Id, cancellationToken);

            return true;
        }

        public async Task<List<KanbanTask>> GetPaginatedTasksAsync(CancellationToken cancellationToken,
            string? status,
            int page,
            int size,
            List<string>? sortFields)
        {
            //service should not be responsible for creating queries, but the logic for pagination and sorting is a bit in its responsability
            //if it would be a kanban -specific repository (not generic repository), it would make more sense to have it there, but since we have only generic repository, this is the best place for it
            var query = _taskRepo.GetQueryableEntities();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusEnum>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
            
            //Sorting
            query = ApplySorting(query, sortFields);

            //Pagination
            if (size > 0 && page >= 0)
            {
                query = query.Skip(page * size).Take(size);
            }

            var items = await _taskRepo.GetEntitiesBasedOn(query, cancellationToken);

            return items;
        }

        public async Task<KanbanTask?> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _taskRepo.FindAsync(id, cancellationToken);
        }

        public async Task<bool> PartialUpdateTaskAsync(int id, PartialUpdateKanbanTaskRequest taskRequest, CancellationToken cancellationToken)
        {
            CheckUpdateParametersAndThrowException(id, taskRequest);

            var foundTask = await _taskRepo.FindAsync(id, cancellationToken);
            if (foundTask == null)
                return false;

            // Update only properties that were sent (NOT NULL)
            _mapper.Map(taskRequest, foundTask);

            _taskRepo.Update(foundTask);

            await _unitOfWork.SaveAsync(cancellationToken);

            await _notifications.TaskUpdated(foundTask, cancellationToken);

            return true;
        }

       
        public async Task<bool> UpdateTaskAsync(int id, FullUpdateKanbanTaskRequest taskRequest, CancellationToken cancellationToken)
        {
            if (id == 0)
                throw new ArgumentException("ID with 0 doesn't exist.");
            if (taskRequest == null)
                throw new ArgumentException("taskRequest parameter with update data cannot be NULL.");


            var foundTask = await _taskRepo.FindAsync(id, cancellationToken);
            if (foundTask == null)
            {
                return false;
            }

            _mapper.Map(taskRequest, foundTask);

            _taskRepo.Update(foundTask);
            await _unitOfWork.SaveAsync(cancellationToken);

            await _notifications.TaskUpdated(foundTask, cancellationToken);

            return true;
        }


        #region Private methods

        private static void CheckUpdateParametersAndThrowException(int id, PartialUpdateKanbanTaskRequest taskRequest)
        {
            if (id == 0)
                throw new ArgumentException("ID with 0 doesn't exist.");
            if (taskRequest == null)
                throw new ArgumentException("taskRequest parameter with update data cannot be NULL.");
        }


        private IQueryable<KanbanTask> ApplySorting(IQueryable<KanbanTask> query, List<string>? sortFields)
        {
            if (sortFields == null || sortFields.Count == 0)
                return query.OrderBy(t => t.Id); // sort by Id by default

            IOrderedQueryable<KanbanTask>? orderedQuery = null;

            foreach (var sort in sortFields)
            {
                var parts = sort.Split(',');
                var field = parts[0].Trim().ToLowerInvariant(); ;
                var direction = (parts.Length > 1 ? parts[1] : "asc").Trim().ToLower();

                orderedQuery = ApplySingleSort(orderedQuery, query, field, direction);
            }

            return orderedQuery ?? query;
        }

        private IOrderedQueryable<KanbanTask> ApplySingleSort(IOrderedQueryable<KanbanTask>? orderedQuery, IQueryable<KanbanTask> query, string field, string direction)
        {
            // First sort - aka Never sorted before
            if (orderedQuery == null)
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
            
            //was already sorted by some field
            return (field, direction) switch
            {
                ("name", "asc") => orderedQuery.ThenBy(t => t.Name),
                ("name", "desc") => orderedQuery.ThenByDescending(t => t.Name),

                ("priority", "asc") => orderedQuery.ThenBy(t => t.PriorityEnum),
                ("priority", "desc") => orderedQuery.ThenByDescending(t => t.PriorityEnum),

                ("status", "asc") => orderedQuery.ThenBy(t => t.Status),
                ("status", "desc") => orderedQuery.ThenByDescending(t => t.Status),

                ("size", "asc") => orderedQuery.ThenBy(t => t.Size),
                ("size", "desc") => orderedQuery.ThenByDescending(t => t.Size),

                _ => orderedQuery.OrderBy(t => t.Id)
            };
        }
        #endregion
    }
}

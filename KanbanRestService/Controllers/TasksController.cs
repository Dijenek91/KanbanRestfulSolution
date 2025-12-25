using AutoMapper;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Factories;
using KanbanRestService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class TasksController : ControllerBase
    {
        private ITaskService _taskService;
        private ITaskDTOFactory _responseDtoFactory;

        public TasksController(ITaskService taskService, ITaskDTOFactory taskDTOFactory, CancellationToken cancellationToken)
        {
            _taskService = taskService;
            _responseDtoFactory = taskDTOFactory;
        }

        /// <summary>
        ///     GET: TasksController/api/tasks 
        ///     pagination example: TasksController/api/tasks?page=0&size=10
        ///     sorting example: TasksController/api/tasks?sort=Name,desc&sort=Size,asc
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        // 
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<KanbanTaskResponse>>> GetAll(
            CancellationToken cancellationToken,
            [FromQuery] string? status,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10,            
            [FromQuery] List<string>? sort = null            
            )
        {
            var listOfTasks = await _taskService.GetPaginatedTasksAsync(cancellationToken, status, page, size, sort);

            var tasksWithHateoasLinks = listOfTasks.Select(task => 
                    _responseDtoFactory.CreateFoundTaskWithHateoas(task.Id, task, Url, Request.Scheme)).ToList();

            var newPagedTasks = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(tasksWithHateoasLinks, tasksWithHateoasLinks.Count(), page, size);

            _responseDtoFactory.AddPagedHateoasLinksFor(newPagedTasks, status, page, size, sort, Url, Request.Scheme);

            return Ok(newPagedTasks);
        }

        

        // GET: TasksController/api/tasks/id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var foundTask = await _taskService.GetTaskByIdAsync(id, cancellationToken);

            TaskExistsOrThrowException(id, foundTask != null);

            var foundTaskDto = _responseDtoFactory.CreateFoundTaskWithHateoas(id, foundTask, Url, Request.Scheme);

            return Ok(foundTaskDto);
        }


        // POST: TasksController/api/tasks CREATE
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateKanbanTaskRequest createTaskDTO, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTask = await _taskService.CreateTaskAsync(createTaskDTO, cancellationToken);

            var taskResponse = _responseDtoFactory.CreateFoundTaskWithHateoas(createdTask.Id, createdTask, Url, Request.Scheme);

            return CreatedAtAction(nameof(GetById), new { id = taskResponse.Id }, taskResponse);
        }

        // PUT: TasksController/api/tasks/id 
        [HttpPut("{id}")]
        public async Task<ActionResult> EditFullUpdate(int id, [FromBody] FullUpdateKanbanTaskRequest fullUpdateTaskDTO, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isTaskCreated = await _taskService.UpdateTaskAsync(id, fullUpdateTaskDTO, cancellationToken);

            TaskExistsOrThrowException(id, isTaskCreated);

            return NoContent(); 
        }

        // PATCH: TasksController/api/tasks/id
        [HttpPatch("{id}")]
        public async Task<ActionResult> EditPartialUpdate(int id, [FromBody] PartialUpdateKanbanTaskRequest partialUpdateTaskDTO, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isTaskCreated = await _taskService.PartialUpdateTaskAsync(id, partialUpdateTaskDTO, cancellationToken);

            TaskExistsOrThrowException(id, isTaskCreated);

            return NoContent();
        }

        // DELETE: TasksController/api/tasks/id
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var resultOfDeletion = await _taskService.DeleteTaskAsync(id, cancellationToken);
            
            TaskExistsOrThrowException(id, resultOfDeletion);
            
            return NoContent();
        }

        #region Private methods

        private static void TaskExistsOrThrowException(int id, bool taskExists)
        {
            if (taskExists == false)
                throw new KeyNotFoundException($"Task with id {id} not found.");
        }
       
        #endregion
    }
}

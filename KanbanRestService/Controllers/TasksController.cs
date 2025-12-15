using AutoMapper;
using KanbanModel.DTOs;
using KanbanModel.ModelClasses;
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
        private readonly IMapper _mapper;

        public TasksController(ITaskService taskService, IMapper mapper)
        {
            _taskService = taskService;
            _mapper = mapper;
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
        public async Task<ActionResult<List<KanbanTask>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] int page = 0,
            [FromQuery] int size = 10,
            [FromQuery] List<string>? sort = null
            )
        {
            var paginatedTasks = await _taskService.GetPaginatedTasksAsync(status, page, size, sort);
            return Ok(paginatedTasks);
        }

        // GET: TasksController/api/tasks/id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var foundTask = await _taskService.GetTaskByIdAsync(id);
            if (foundTask == null)
            {
                return NotFound();
            }
            return Ok(foundTask);
        }

        // POST: TasksController/api/tasks CREATE
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateKanbanTaskDTO createTaskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = _mapper.Map<KanbanTask>(createTaskDTO); //dto conversion to entity

            var createdTask = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);            
        }

        // PUT: TasksController/api/tasks/ID 
        [HttpPut("{id}")]
        public async Task<ActionResult> EditFullUpdate(int id, [FromBody] FullUpdateKanbanTaskDTO fullUpdateTaskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = _mapper.Map<KanbanTask>(fullUpdateTaskDTO);

            var isTaskCreated = await _taskService.UpdateTaskAsync(id, task);
            
            if (isTaskCreated == false)
                return NotFound($"Task {id} not found for update.");

            return NoContent(); 
        }

        // PATCH: TasksController/api/tasks/id
        [HttpPatch("{id}")]
        public async Task<ActionResult> EditPartialUpdate(int id, [FromBody] PartialUpdateKanbanTaskDTO partialUpdateTaskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = _mapper.Map<KanbanTask>(partialUpdateTaskDTO);

            var isTaskCreated = await _taskService.PartialUpdateTaskAsync(id, task);

            if (isTaskCreated == false)
                return NotFound($"Task {id} not found for partial update.");

            return NoContent();
        }

       

        // DELETE: TasksController/api/tasks/id
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultOfDeletion = await _taskService.DeleteTaskAsync(id);
            if(resultOfDeletion == false)
            {
                return NotFound($"Task with {id} not found for deletion");
            }
            return NoContent();
        }
    }
}

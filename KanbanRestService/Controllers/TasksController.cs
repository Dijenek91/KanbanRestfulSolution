using KanbanModel.DTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Services;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: TasksController/api/tasks
        [HttpGet]
        public async Task<ActionResult<List<KanbanTask>>> GetAll()
        {
            return Ok(await _taskService.GetAllTasksAsync());
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
        public async Task<ActionResult> Create([FromBody] CreateKanbanTaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = new KanbanTask
            {
                Name = taskDTO.Name,
                Description = taskDTO.Description,
                Status = taskDTO.Status,
                Size = taskDTO.Size,
                PriorityEnum = taskDTO.PriorityEnum
            };

            var createdTask = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);            
        }

        // PUT: TasksController/api/tasks/ID 
        [HttpPut("{id}")]
        public async Task<ActionResult> EditFullUpdate(int id, [FromBody] FullUpdateKanbanTaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = new KanbanTask
            {
                Name = taskDTO.Name,
                Description = taskDTO.Description,
                Status = taskDTO.Status,
                Size = taskDTO.Size,
                PriorityEnum = taskDTO.PriorityEnum
            };

            var isTaskCreated = await _taskService.UpdateTaskAsync(id, task);
            
            if (isTaskCreated == false)
                return NotFound($"Task {id} not found for update.");

            return NoContent(); 
        }

        // PATCH: TasksController/api/tasks/id
        [HttpPatch("{id}")]
        public async Task<ActionResult> EditPartialUpdate(int id, [FromBody] PartialUpdateKanbanTaskDTO taskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = CreateKanbanTaskBasedOnPartialUpdateTaskDTO(taskDTO);

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

        
        #region private methods
        private static KanbanTask CreateKanbanTaskBasedOnPartialUpdateTaskDTO(PartialUpdateKanbanTaskDTO taskDTO)
        {
            var task = new KanbanTask();

            if (taskDTO.Name != null)
                task.Name = taskDTO.Name;
            if (taskDTO.Description != null)
                task.Description = taskDTO.Description;
            if (taskDTO.Status.HasValue)
                task.Status = taskDTO.Status;
            if (taskDTO.Size != null)
                task.Size = taskDTO.Size;
            if (taskDTO.PriorityEnum.HasValue)
                task.PriorityEnum = taskDTO.PriorityEnum;

            return task;
        }
        #endregion

    }
}

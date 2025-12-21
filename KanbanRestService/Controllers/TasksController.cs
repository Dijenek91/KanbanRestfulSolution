using AutoMapper;
using KanbanModel.DTOs;
using KanbanModel.DTOs.ReturnDTOs;
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
            var listOfTasks = await _taskService.GetPaginatedTasksAsync(status, page, size, sort);

            var tasksWithHateoasLinks = listOfTasks.Select(task => CreateFoundTaskWithHateoas(task.Id, task)).ToList();

            var newPagedTasks = new PagedResultDTO<KanbanTaskDTO>(tasksWithHateoasLinks, tasksWithHateoasLinks.Count(), page, size);

             _addPagedHateoasLinksFor(newPagedTasks, status, page, size, sort);

            return Ok(newPagedTasks);
        }

        

        // GET: TasksController/api/tasks/id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var foundTask = await _taskService.GetTaskByIdAsync(id);

            TaskExistsOrThrowException(id, foundTask != null);

            var foundTaskDto = CreateFoundTaskWithHateoas(id, foundTask);

            return Ok(foundTaskDto);
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

        // PUT: TasksController/api/tasks/id 
        [HttpPut("{id}")]
        public async Task<ActionResult> EditFullUpdate(int id, [FromBody] FullUpdateKanbanTaskDTO fullUpdateTaskDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = _mapper.Map<KanbanTask>(fullUpdateTaskDTO);

            var isTaskCreated = await _taskService.UpdateTaskAsync(id, task);

            TaskExistsOrThrowException(id, isTaskCreated);

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

            TaskExistsOrThrowException(id, isTaskCreated);

            return NoContent();
        }

        // DELETE: TasksController/api/tasks/id
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultOfDeletion = await _taskService.DeleteTaskAsync(id);
            
            TaskExistsOrThrowException(id, resultOfDeletion);
            
            return NoContent();
        }

        #region Private methods

        private static void TaskExistsOrThrowException(int id, bool taskExists)
        {
            if (taskExists == false)
                throw new KeyNotFoundException($"Task with id {id} not found.");
        }

        //these can be added in a seperate facotory class
        private KanbanTaskDTO CreateFoundTaskWithHateoas(int id, KanbanTask? foundTask)
        {
            var foundTasktDto = _mapper.Map<KanbanTaskDTO>(foundTask);
            
            var getHrefString = Url.Action(nameof(GetById), "Tasks", new { id }, Request.Scheme);
            var editHrefString = Url.Action(nameof(EditFullUpdate), "Tasks", new { id }, Request.Scheme);
            var partialEditHrefString = Url.Action(nameof(EditPartialUpdate), "Tasks", new { id }, Request.Scheme);
            var DeleteHrefString = Url.Action(nameof(Delete), "Tasks", new { id }, Request.Scheme);

            foundTasktDto.Links.Add(new LinkDTO("self", getHrefString, "GET"));
            foundTasktDto.Links.Add(new LinkDTO("update", editHrefString, "PUT"));
            foundTasktDto.Links.Add(new LinkDTO("partial update", partialEditHrefString, "PATCH"));
            foundTasktDto.Links.Add(new LinkDTO("delete", DeleteHrefString, "DELETE"));

            return foundTasktDto;
        }

        private void _addPagedHateoasLinksFor(PagedResultDTO<KanbanTaskDTO> newPagedTasks, string? status, int page, int size, List<string>? sort)
        {
            var selfUrl = Url.Action(nameof(GetAll), "Tasks", new { status, page, size, sort }, Request.Scheme);
            var createUrl = Url.Action(nameof(Create), "Tasks", null, Request.Scheme);
            var nextUrl = Url.Action(nameof(GetAll), "Tasks", new { status, page = page + 1, size, sort }, Request.Scheme);
            var prevUrl = Url.Action(nameof(GetAll), "Tasks", new { status, page = page > 0 ? page - 1 : 0, size, sort }, Request.Scheme);

            newPagedTasks.Links.Add(new LinkDTO("self", selfUrl, "GET"));
            newPagedTasks.Links.Add(new LinkDTO("create", createUrl, "POST"));
            newPagedTasks.Links.Add(new LinkDTO("next", nextUrl, "GET"));
            newPagedTasks.Links.Add(new LinkDTO("prev", prevUrl, "GET"));
        }
        #endregion
    }
}

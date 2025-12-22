using AutoMapper;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Factories
{
    public class TaskDTOFactory : ITaskDTOFactory
    {

        private readonly IMapper _mapper;

        public TaskDTOFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        //To many parameters makes using this function a bit complicated, but it is necessary to create the HATEOAS links
        //and remove code complexity from the controller
        public KanbanTaskResponse CreateFoundTaskWithHateoas(int id,
            KanbanTask? task, 
            IUrlHelper url, 
            string requestScheme)
        {
            var foundTasktDto = _mapper.Map<KanbanTaskResponse>(task);

            var getHrefString = url.Action("GetById", "Tasks", new { id }, requestScheme);
            var editHrefString = url.Action("EditFullUpdate", "Tasks", new { id }, requestScheme);
            var partialEditHrefString = url.Action("EditPartialUpdate", "Tasks", new { id }, requestScheme);
            var deleteHrefString = url.Action("Delete", "Tasks", new { id }, requestScheme);

            foundTasktDto.Links.Add(new LinkDTO("self", getHrefString, "GET"));
            foundTasktDto.Links.Add(new LinkDTO("update", editHrefString, "PUT"));
            foundTasktDto.Links.Add(new LinkDTO("partial update", partialEditHrefString, "PATCH"));
            foundTasktDto.Links.Add(new LinkDTO("delete", deleteHrefString, "DELETE"));

            return foundTasktDto;
        }

        public void AddPagedHateoasLinksFor(PagedResultKanbanTasksResponse<KanbanTaskResponse> newPagedTasks,
            string? status,
            int page,
            int size,
            List<string>? sort,
            IUrlHelper url,
            string requestScheme
            )
        {
            var selfUrl = url.Action("GetAll", "Tasks", new { status, page, size, sort }, requestScheme);
            var createUrl = url.Action("Create", "Tasks", null, requestScheme);
            var nextUrl = url.Action("GetAll", "Tasks", new { status, page = page + 1, size, sort }, requestScheme);
            var prevUrl = url.Action("GetAll", "Tasks", new { status, page = page > 0 ? page - 1 : 0, size, sort }, requestScheme);

            newPagedTasks.Links.Add(new LinkDTO("self", selfUrl, "GET"));
            newPagedTasks.Links.Add(new LinkDTO("create", createUrl, "POST"));
            newPagedTasks.Links.Add(new LinkDTO("next", nextUrl, "GET"));
            newPagedTasks.Links.Add(new LinkDTO("prev", prevUrl, "GET"));
        }
    }
}

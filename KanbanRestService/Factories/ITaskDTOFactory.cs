using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Factories
{
    public interface ITaskDTOFactory
    {
        public KanbanTaskResponse CreateFoundTaskWithHateoas(int id,
            KanbanTask? task,
            IUrlHelper url,
            string requestScheme);

        public void AddPagedHateoasLinksFor(PagedResultKanbanTasksResponse<KanbanTaskResponse> newPagedTasks,
                    string? status,
                    int page,
                    int size,
                    List<string>? sort,
                    IUrlHelper url,
                    string requestScheme
                    );
    }
}

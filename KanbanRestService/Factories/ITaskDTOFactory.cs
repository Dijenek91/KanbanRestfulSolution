using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.AspNetCore.Mvc;

namespace KanbanRestService.Factories
{
    public interface ITaskDTOFactory
    {
        public KanbanTaskResponse CreateFoundTaskWithHateoas(
            int id,
            KanbanTask? task,
            IUrlHelper url,
            string requestScheme);

        public List<KanbanTaskResponse> CreateListFoundTasksWithHateoas(
            List<KanbanTask?> foundTasks,
            IUrlHelper url,
            string requestScheme);

        public PagedResultKanbanTasksResponse<KanbanTaskResponse> CreatePagedResult_WithHateoasLinksFor(
                    List<KanbanTaskResponse> tasksWithHateoasLinks,
                    string? status,
                    int page,
                    int size,
                    List<string>? sort,
                    IUrlHelper url,
                    string requestScheme
                    );
    }
}

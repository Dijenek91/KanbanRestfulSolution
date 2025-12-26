using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Services;

namespace KanbanRestService.GraphQL.Mutations
{
    public class TaskMutation
    {
        public async Task<KanbanTask> CreateTask(
            CreateKanbanTaskRequest createKanbanTask,
            [Service] ITaskService taskService,
            CancellationToken cancellationToken)
        {
            return await taskService.CreateTaskAsync(createKanbanTask, cancellationToken);
        }


        public async Task<bool> FullUpdateTask(
            int id,
            FullUpdateKanbanTaskRequest updateKanbanTask,
            [Service] ITaskService taskService,
            CancellationToken cancellationToken)
        {
            return await taskService.UpdateTaskAsync(id, updateKanbanTask, cancellationToken);
        }

        public async Task<bool> PartialUpdateTask(
            int id,
            PartialUpdateKanbanTaskRequest partialUpdateKanbanTask,
            [Service] ITaskService taskService,
            CancellationToken cancellationToken)
        {
            return await taskService.PartialUpdateTaskAsync(id, partialUpdateKanbanTask, cancellationToken);
        }

        public async Task<bool> DeleteUpdateTask(
            int id,
            [Service] ITaskService taskService,
            CancellationToken cancellationToken)
        {
            return await taskService.DeleteTaskAsync(id, cancellationToken);
        }
    }
}

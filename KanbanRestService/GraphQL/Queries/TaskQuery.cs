using KanbanInfrastructure.DAL;
using KanbanModel.ModelClasses;
using Microsoft.EntityFrameworkCore;

namespace KanbanRestService.GraphQL.Queries
{
    public class TaskQuery
    {
        [UsePaging]
        [UseFiltering]
        [UseSorting]
        public IQueryable<KanbanTask> GetTasks(
            [Service] KanbanAppDbContext kanbanDbContext)
        {
            return kanbanDbContext.KanbanTasks.AsNoTracking();//execution happens in HotChocolate pipeline
        }

        public async Task<KanbanTask?> GetTaskById(
            int id,
           [Service] KanbanAppDbContext kanbanDbContext,
           CancellationToken cancelationToken)
        {
            return await kanbanDbContext.KanbanTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancelationToken);
        }
    }
}

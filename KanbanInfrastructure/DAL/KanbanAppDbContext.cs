using KanbanModel.ModelClasses;
using Microsoft.EntityFrameworkCore;

namespace KanbanInfrastructure.DAL
{
    public class KanbanAppDbContext : DbContext
    {
        public KanbanAppDbContext(DbContextOptions<KanbanAppDbContext> options) : base(options)
        {
        }

        public DbSet<KanbanTask> KanbanTasks { get; set; }
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace KanbanInfrastructure.DAL
{
    /// <summary>
    /// Since my DbContext is in a different project than my startup project, 
    /// I need to implement IDesignTimeDbContextFactory to help EF Core tools
    /// This is only used during migrations at design time
    /// program.cs in KanbanRestService is used at runtime so that the Service and Controllers can use my DBcontext
    /// </summary>
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KanbanAppDbContext>
    {
        public KanbanAppDbContext CreateDbContext(string[] args)
        {
            // Read connection string from the API project (StartupProject)
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "KanbanRestService"))
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KanbanAppDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new KanbanAppDbContext(optionsBuilder.Options);
        }
    }    
}

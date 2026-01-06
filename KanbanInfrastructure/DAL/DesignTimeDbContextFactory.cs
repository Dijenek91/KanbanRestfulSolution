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
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KanbanAppDbContext>
    {
        public KanbanAppDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            // Read connection string from the API project (StartupProject)
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "KanbanRestService"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            var  optionsBuilder = GetOptionsBuilder(env, config);

            return new KanbanAppDbContext(optionsBuilder.Options);
        }

        private static DbContextOptionsBuilder<KanbanAppDbContext> GetOptionsBuilder(string env, IConfigurationRoot config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanAppDbContext>();
            
            Console.WriteLine($"DesignFactory - Enviroment: {env}");
            Console.WriteLine($"DesignFactory - Db connection string: {connectionString}");
            
            if (env.Equals("Docker",StringComparison.InvariantCultureIgnoreCase))
            {
                optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("MigrationPostgresSql"));
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("MigrationSqlServer"));
            }

            return optionsBuilder;
        }
    }    
}

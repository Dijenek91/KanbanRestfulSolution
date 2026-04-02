// CSharp tests/Integration/WebAppFactoryCustom.cs
using KanbanInfrastructure.DAL;
using KanbanRestService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class WebAppFactoryCustom : WebApplicationFactory<Program>
{
    private SqliteConnection _connection = null;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                ["SuperSecretJwtKey"] = "supersecretkey12345678901234567890",
                ["Issuer"] = "MyKanbanTaskApp",
                ["Audience"] = "MyKanbanTaskApp"
            };

            configBuilder.AddInMemoryCollection(testConfig!);
        });

        builder.ConfigureServices(services =>
        {
            // Create and open shared connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Replace DbContext to use SAME connection
            services.AddDbContext<KanbanAppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Build provider and initialize DB
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KanbanAppDbContext>();

            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
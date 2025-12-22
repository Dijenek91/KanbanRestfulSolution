using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.DTOs.Mapping;
using KanbanRestService.Factories;
using KanbanRestService.Hubs;
using KanbanRestService.Middlware;
using KanbanRestService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace KanbanRestService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //DB related services
            builder.Services.AddDbContext<KanbanAppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IUnitOfWork<KanbanAppDbContext>, GenericUnitOfWork<KanbanAppDbContext>>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Controller related services
            builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile(new TaskProfile());
                });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ITaskDTOFactory, TaskDTOFactory>();
            builder.Services.AddScoped<ITaskService, TaskServiceHost>();

            builder.Services.AddSignalR();

            AddSecurityTo(builder);

            AddCorsToBuilder(builder);

            // HTTP request pipeline configuration
            var app = builder.Build();            

            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseRouting();

            SetupCorsAndOpenApiScalar(app);

            app.MapHub<TasksHub>("/taskHub");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void AddSecurityTo(WebApplicationBuilder builder)
        {

            var keyBytes = Encoding.UTF8.GetBytes(builder.Configuration["SuperSecretJwtKey"]);
            builder.Services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }
                ).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Issuer"],
                        ValidAudience = builder.Configuration["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                    };
                });

            if (!builder.Environment.IsDevelopment())
            {
                return;
            }
        
        }
        

        private static void AddCorsToBuilder(WebApplicationBuilder builder)
        {
            /** Production CORS Configuration **/
            var allowedOrigins = new[]
            {
                "https://mykanbanapp.com",
                "https://www.mykanbanapp.com"
            };

            builder.Services.AddCors(options =>
            {
                //Development cors configuration
                options.AddPolicy("DevCors", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });

                // Production CORS (restricted)
                options.AddPolicy("ProdCors", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        private static void SetupCorsAndOpenApiScalar(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.UseCors("DevCors");
            }
            else
            {
                app.UseCors("ProdCors");
            }
        }
    }
}

using KanbanInfrastructure.DAL;
using KanbanIntegrationTests.CustomTestSupportItems;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace KanbanIntegrationTests
{
    [TestFixture]
    internal class TaskControllerIntegrationTests
    {
        private WebAppFactoryCustom _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _factory = new WebAppFactoryCustom();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            var token = TestJwtHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Result);
        }

        private async Task SeedAsync(List<KanbanTask> tasks)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KanbanAppDbContext>();

            db.KanbanTasks.RemoveRange(db.KanbanTasks); // clean
            await db.SaveChangesAsync();

            db.KanbanTasks.AddRange(tasks);
            await db.SaveChangesAsync();
        }

        private async Task SeedEmptyAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KanbanAppDbContext>();

            db.KanbanTasks.RemoveRange(db.KanbanTasks); // clean
            await db.SaveChangesAsync();
        }

        #region GetAll Tests

        [Test]
        public async Task GetAll_ReturnsSeededTasks()
        {
            List<KanbanTask> kanbanTasks = new List<KanbanTask>() 
            { 
                new KanbanTask 
                {
                    Name = "Task1",
                    Description = "Description1",
                    Status = StatusEnum.ToDo
                },
                new KanbanTask 
                {
                    Name = "Task2",
                    Description = "Description2",
                    Status = StatusEnum.ToDo
                }
            };

            await SeedAsync(kanbanTasks);            

            var response = await _client.GetAsync("/api/tasks");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>().Result;
            Assert.That(taskReponse.Items.Count, Is.EqualTo(kanbanTasks.Count));
        }

        [Test]
        public async Task GetAll_ReturnsNoTasks()
        {
            await SeedEmptyAsync();

            var response = await _client.GetAsync("/api/tasks");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>().Result;
            Assert.That(taskReponse.Items.Count, Is.EqualTo(0));
        }

        #endregion

        #region GetById task
        [Test]
        public async Task GetById_ReturnsTask()
        {
            List<KanbanTask> kanbanTasks = new List<KanbanTask>()
            {
                new KanbanTask
                {
                    Id = 0,
                    Name = "Task1",
                    Description = "Description1",
                    Status = StatusEnum.ToDo
                },
                new KanbanTask
                {
                    Id = 1,
                    Name = "Task2",
                    Description = "Description2",
                    Status = StatusEnum.ToDo
                }
            };

            await SeedAsync(kanbanTasks);
            
            var response = await _client.GetAsync("/api/tasks/1");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = response.Content.ReadFromJsonAsync<KanbanTaskResponse>().Result;
            Assert.That(taskReponse.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetById_ReturnsNoTask()
        {
            List<KanbanTask> kanbanTasks = new List<KanbanTask>()
            {
                new KanbanTask
                {
                    Name = "Task1",
                    Description = "Description1",
                    Status = StatusEnum.ToDo
                },
                new KanbanTask
                {
                    Name = "Task2",
                    Description = "Description2",
                    Status = StatusEnum.ToDo
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks/5");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var taskReponse = response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>().Result;
            Assert.That(taskReponse.Items, Is.Null);            
        }
        #endregion

        #region Create

        [Test]
        public async Task CreateTask_Returns201Created()
        {
            await SeedEmptyAsync();

            var request = new CreateKanbanTaskRequest()
            {
                Name = "new Task",
                Description = "new descr",
                PriorityEnum = PriorityEnum.Medium,
                Size = 3,
                Status = StatusEnum.ToDo
            };

            var response = await _client.PostAsJsonAsync("/api/tasks", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var taskReponse = response.Content.ReadFromJsonAsync<KanbanTaskResponse>().Result;
            Assert.That(taskReponse.Name, Is.EqualTo(request.Name));
        }

        [Test]
        public async Task CreateTask_BadRequest()
        {
            await SeedEmptyAsync();

            var request = new CreateKanbanTaskRequest()
            {
                Name = "new Task",
                Description = "new descr",
                PriorityEnum = PriorityEnum.Medium,
                Size = 0, //invalid model -> throws restriction
                Status = StatusEnum.ToDo
            };

            var response = await _client.PostAsJsonAsync("/api/tasks", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        #endregion
    }
}

using KanbanInfrastructure.DAL;
using KanbanIntegrationTests.CustomTestSupportItems;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

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
        public async Task Setup()
        {
            var token = await TestJwtHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

        #region Authentication
        
        [Test]
        public async Task GetAll_WithoutToken_Returns401Unauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync("/api/tasks");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetAll_InvalidToken_Returns401Unauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid token");

            var response = await _client.GetAsync("/api/tasks");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        #endregion

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
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(kanbanTasks.Count));

            Assert.That(taskReponse.Links.Any(link => link.Rel == "self"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "create"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "next"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "prev"), Is.True);
        }

        [Test]
        public async Task GetAll_FilterByStatus_ReturnsStatusToDoTasks()
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
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks?status=ToDo");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAll_FilterByInvalidStatus_ReturnsAllTasks()
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
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks?status=INVALIDSTATUS");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(kanbanTasks.Count));
        }

        [Test]
        public async Task GetAll_FilterByPage_Returns2ndPageTasks()
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
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                },
                new KanbanTask
                {
                    Name = "Task4",
                    Description = "Description4",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks?page=1&size=2");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(2));

            Assert.That(taskReponse.Links.Any(link => link.Rel == "next"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "prev"), Is.True);
        }

        [Test]
        public async Task GetAll_Sort_AscName()
        {
            List<KanbanTask> unsortedKanbanTasks = new List<KanbanTask>()
            {
                new KanbanTask
                {
                    Name = "Task4",
                    Description = "Description4",
                    Status = StatusEnum.ToDo
                },
                new KanbanTask
                {
                    Name = "Task2",
                    Description = "Description2",
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                },
                new KanbanTask
                {
                    Name = "Task1",
                    Description = "Description1",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(unsortedKanbanTasks);

            var response = await _client.GetAsync("/api/tasks?sort=Name,asc");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(4));

            Assert.IsTrue(taskReponse.Items.SequenceEqual(taskReponse.Items.OrderBy(task => task.Name)));
        }

        [Test]
        public async Task GetAll_InvalidSortField_ReturnsAllUnsortedTasks()
        {
            List<KanbanTask> unsortedKanbanTasks = new List<KanbanTask>()
            {
                new KanbanTask
                {
                    Name = "Task4",
                    Description = "Description4",
                    Status = StatusEnum.ToDo
                },
                new KanbanTask
                {
                    Name = "Task2",
                    Description = "Description2",
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                },
                new KanbanTask
                {
                    Name = "Task1",
                    Description = "Description1",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(unsortedKanbanTasks);

            var response = await _client.GetAsync("/api/tasks?sort=INVALIDFIELD,asc");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(4));

            Assert.IsTrue(taskReponse.Items.SequenceEqual(taskReponse.Items));
        }

        [Test]
        public async Task GetAll_FilterByInvalidPage_IgnoresPaging_ReturnsAllTasks()
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
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                },
                new KanbanTask
                {
                    Name = "Task4",
                    Description = "Description4",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks?page=-1&size=2");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(kanbanTasks.Count));
        }

        [Test]
        public async Task GetAll_FilterBySize_IgnoresPaging_ReturnsAllTasks()
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
                    Status = StatusEnum.Completed
                },
                new KanbanTask
                {
                    Name = "Task3",
                    Description = "Description3",
                    Status = StatusEnum.InProgress
                },
                new KanbanTask
                {
                    Name = "Task4",
                    Description = "Description4",
                    Status = StatusEnum.InProgress
                }
            };

            await SeedAsync(kanbanTasks);

            var response = await _client.GetAsync("/api/tasks?page=1&size=0");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = await response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            Assert.That(taskReponse.TotalCount, Is.EqualTo(kanbanTasks.Count));
        }


        [Test]
        public async Task GetAll_ReturnsNoTasks()
        {
            await SeedEmptyAsync();

            var response = await _client.GetAsync("/api/tasks");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var taskReponse = response.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>().Result;
            Assert.That(taskReponse.Items.Count, Is.EqualTo(0));

            Assert.That(taskReponse.Links.Any(link => link.Rel == "self"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "create"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "next"), Is.False);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "prev"), Is.False);
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
            var taskReponse = await response.Content.ReadFromJsonAsync<KanbanTaskResponse>();
            Assert.That(taskReponse.Id, Is.EqualTo(1));

            Assert.That(taskReponse.Links.Any(link => link.Rel == "self"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "update"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "partial update"), Is.True);
            Assert.That(taskReponse.Links.Any(link => link.Rel == "delete"), Is.True);
        }

        [Test]
        public async Task GetById_ReturnsNoTask()
        {
            await SeedEmptyAsync();

            var response = await _client.GetAsync("/api/tasks/253");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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

            var httpPostResponse = await _client.PostAsJsonAsync("/api/tasks", request);
            var taskResponse = await httpPostResponse.Content.ReadFromJsonAsync<KanbanTaskResponse>();

            Assert.That(httpPostResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var getResponse = await _client.GetAsync("/api/tasks");

            var result = await getResponse.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();

            Assert.That(result.Items.Count, Is.EqualTo(1));

            Assert.That(taskResponse.Links.Any(link => link.Rel == "self"), Is.True);
            Assert.That(taskResponse.Links.Any(link => link.Rel == "update"), Is.True);
            Assert.That(taskResponse.Links.Any(link => link.Rel == "partial update"), Is.True);
            Assert.That(taskResponse.Links.Any(link => link.Rel == "delete"), Is.True);
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

        #region FullUpdate

        [Test]
        public async Task UpdateFullTask_ReturnsNoContent()
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

            var getResponse = await _client.GetAsync("/api/tasks");

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseResult = await getResponse.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            List<int> taskIds = responseResult.Items.Select(task => task.Id).ToList();

            var request = new FullUpdateKanbanTaskRequest()
            {
                Name = "Task1",
                Description = "changed descr",
                PriorityEnum = PriorityEnum.Medium,
                Size = 3,
                Status = StatusEnum.ToDo
            };

            var response = await _client.PutAsJsonAsync($"/api/tasks/{taskIds[0]}", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task UpdateFullTask_BadRequest()
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

            var badRequest = new FullUpdateKanbanTaskRequest()
            {
                Name = "Task1",
                Description = "changed descr",
                PriorityEnum = PriorityEnum.Medium,
                Size = 0, // invalid model -> throws restriction
                Status = StatusEnum.ToDo
            };

            var response = await _client.PutAsJsonAsync("/api/tasks/1", badRequest);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateFullTask_NotFound()
        {
            await SeedEmptyAsync();

            var request = new FullUpdateKanbanTaskRequest()
            {
                Name = "Task1",
                Description = "changed descr",
                PriorityEnum = PriorityEnum.Medium,
                Size = 1, 
                Status = StatusEnum.ToDo
            };

            var response = await _client.PutAsJsonAsync("/api/tasks/1", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        #endregion

        #region Partial Update

        [Test]
        public async Task UpdatePartialTask_ReturnsNoContent()
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

            var getResponse = await _client.GetAsync("/api/tasks");

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseResult = await getResponse.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            List<int> taskIds = responseResult.Items.Select(task => task.Id).ToList();

            var request = new PartialUpdateKanbanTaskRequest()
            {
                Description = "changed descr"
            };

            var response = await _client.PatchAsJsonAsync($"/api/tasks/{taskIds[0]}", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var getResponse2 = await _client.GetAsync($"/api/tasks/{taskIds[0]}");
            var result = await getResponse2.Content.ReadFromJsonAsync<KanbanTaskResponse>();

            Assert.That(result.Description, Is.EqualTo(request.Description));
        }

        [Test]
        public async Task UpdatePartialTask_BadRequest()
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

            var badRequest = new PartialUpdateKanbanTaskRequest()
            {
                Size = 0
            };

            var response = await _client.PatchAsJsonAsync("/api/tasks/1", badRequest);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdatePartialTask_NotFound()
        {
            await SeedEmptyAsync();

            var request = new PartialUpdateKanbanTaskRequest()
            {
                Description = "changed descr"
            };

            var response = await _client.PatchAsJsonAsync("/api/tasks/1", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        #endregion

        #region Delete

        [Test]
        public async Task DeleteTask_ReturnsNoContent()
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

            var getResponse = await _client.GetAsync("/api/tasks");

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseResult = await getResponse.Content.ReadFromJsonAsync<PagedResultKanbanTasksResponse<KanbanTaskResponse>>();
            List<int> taskIds = responseResult.Items.Select(task => task.Id).ToList();

            var response = await _client.DeleteAsync($"/api/tasks/{taskIds[0]}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DeleteTask_NotFound_ThrowsException()
        {
            await SeedEmptyAsync();

            var response = await _client.DeleteAsync("/api/tasks/1");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        #endregion
    }
}
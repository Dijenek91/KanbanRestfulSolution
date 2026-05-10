using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Controllers;
using KanbanRestService.Factories;
using KanbanRestService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace KanbanTests.Unit.Controllers
{
    [TestFixture]
    [Category("Unit")]
    internal class TaskControllerTests
    {
        private Mock<ITaskService> _taskServiceMock;
        private Mock<ITaskDTOFactory> _taskResponseFactoryMock;
        private TasksController _taskController;

        [SetUp]
        public void Setup()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _taskResponseFactoryMock = new Mock<ITaskDTOFactory>();

            _taskController = new TasksController(_taskServiceMock.Object, _taskResponseFactoryMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http"; // or "https" if needed

            _taskController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Option A: simple mock IUrlHelper
            _taskController.Url = Mock.Of<IUrlHelper>();
        }

        [Test]
        public void TasksController_Has_Authorize_And_Route_Attributes()
        {
            // assert attributes on controller type
            var t = typeof(TasksController);
            Assert.That(t.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true).Any(), Is.True, "Authorize attribute missing");
            var routeAttr = t.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), inherit: true).Cast<Microsoft.AspNetCore.Mvc.RouteAttribute>().FirstOrDefault();
            Assert.That(routeAttr, Is.Not.Null);
            Assert.That(routeAttr.Template, Is.EqualTo("api/[controller]"));
        }

        #region TaskServiceConstructorValidation

        [Test]
        public void Constructor_NullTaskService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TasksController(null!, _taskResponseFactoryMock.Object));
        }
        [Test]
        public void Constructor_NullTaskFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TasksController(_taskServiceMock.Object, null!));
        }

        #endregion

        #region GetAll

        [Test]
        public void GetAll_ValidRequest_ResponseOK()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed}
                };

            _taskServiceMock.Setup(service => service.GetPaginatedTasksAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(), 
                It.IsAny<int>(),
                It.IsAny<int>(), 
                It.IsAny<List<string>>()))
                .ReturnsAsync(taskList);

            var listOfKanbanTaskResponse = taskList.Select(kanbanTask =>
                new KanbanTaskResponse
                {
                    Id = kanbanTask.Id,
                    Name = kanbanTask.Name,
                    Description = kanbanTask.Description,
                    Status = kanbanTask.Status,
                    Size = kanbanTask.Size,
                    PriorityEnum = kanbanTask.PriorityEnum,
                    Links = new List<LinkDTO>()
                }).ToList();


            _taskResponseFactoryMock.Setup(factory => factory.CreateListFoundTasksWithHateoas(
               It.IsAny<List<KanbanTask?>>(),
               It.IsAny<IUrlHelper>(),
               It.IsAny<string>()))
               .Returns(listOfKanbanTaskResponse);

            var expectedPagedResponse = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(listOfKanbanTaskResponse, listOfKanbanTaskResponse.Count(), 0, 10);
            expectedPagedResponse.Links = new List<LinkDTO>();

            _taskResponseFactoryMock.Setup(factory => factory.CreatePagedResult_WithHateoasLinksFor(
                It.IsAny<List<KanbanTaskResponse>>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<string>>(),
                It.IsAny<IUrlHelper>(),
                It.IsAny<string>()))
                .Returns(expectedPagedResponse);

            //act
            var result = _taskController.GetAll(CancellationToken.None, null, 0, 10, null).Result;

            //assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.SameAs(expectedPagedResponse));

            _taskServiceMock.Verify(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), null, 0, 10, null), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), "http"), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), null, 0, 10, null, It.IsAny<IUrlHelper>(), "http"), Times.Once);
        }

        [Test]
        public async Task GetAll_WhenRequestIsNull_FallsBackToHttpScheme_OrFailsClearly()
        {
            // arrange
            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(tasks);

            var responses = tasks.Select(t => new KanbanTaskResponse { Id = t.Id }).ToList();
            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(responses);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(new PagedResultKanbanTasksResponse<KanbanTaskResponse>(responses, responses.Count, 0, 10));

            // simulate missing Request
            _taskController.ControllerContext = new ControllerContext { HttpContext = null };
            _taskController.Url = Mock.Of<IUrlHelper>();

            // act & assert: choose expected contract. If you want the controller to use fallback "http":
            _taskResponseFactoryMock.Invocations.Clear();
            Assert.DoesNotThrowAsync(async () => await _taskController.GetAll(CancellationToken.None, null, 0, 10, null));
            _taskResponseFactoryMock.Verify(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.Is<string>(s => s == "http")), Times.AtLeastOnce);

            // If instead your policy is to throw, change above to Assert.ThrowsAsync<NullReferenceException>(...)
        }

        [Test]
        public async Task GetAll_WhenNoTasks_ReturnsEmptyPagedResult()
        {
            // arrange
            var emptyTasks = new List<KanbanTask>();
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(emptyTasks);

            var emptyResponses = new List<KanbanTaskResponse>();
            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(emptyResponses);

            var expectedPaged = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(emptyResponses, 0, 0, 10);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedPaged);

            // act
            var action = await _taskController.GetAll(CancellationToken.None, null, 0, 10, null);

            // assert
            var ok = action.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(expectedPaged));
        }

        [Test]
        public async Task GetAll_ForwardsPaginationParametersToServiceAndFactory()
        {
            // arrange
            var page = 2; 
            var size = 5;

            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(tasks);

            var responses = tasks.Select(t => new KanbanTaskResponse { Id = t.Id }).ToList();
            var expectedPaged = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(responses, responses.Count, page, size);

            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(responses);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedPaged);

            // act            
            await _taskController.GetAll(CancellationToken.None, null, page, size, null);

            // assert
            _taskServiceMock.Verify(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), null, page, size, null), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), null, page, size, null, It.IsAny<IUrlHelper>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetAll_ForwardsStatusAndSortParameters()
        {
            // arrange
            var status = "InProgress";
            var sort = new List<string> { "Name,desc" };
            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), status, It.IsAny<int>(), It.IsAny<int>(), sort))
                .ReturnsAsync(tasks);

            var responses = tasks.Select(t => new KanbanTaskResponse { Id = t.Id }).ToList();
            var expectedPaged = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(responses, responses.Count, 0, 10);

            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(responses);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), status, It.IsAny<int>(), It.IsAny<int>(), sort, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedPaged);

            // act
            await _taskController.GetAll(CancellationToken.None, status, 0, 10, sort);

            // assert
            _taskServiceMock.Verify(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), status, 0, 10, sort), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), status, 0, 10, sort, It.IsAny<IUrlHelper>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetAll_UsesRequestSchemeWhenBuildingLinks()
        {
            // arrange
            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(tasks);

            var responses = tasks.Select(t => new KanbanTaskResponse { Id = t.Id }).ToList();
            var expectedPaged = new PagedResultKanbanTasksResponse<KanbanTaskResponse>(responses, responses.Count, 0, 10);

            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(responses);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedPaged);

            // change scheme
            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            _taskController.ControllerContext = new ControllerContext { HttpContext = ctx };

            // act
            await _taskController.GetAll(CancellationToken.None, null, 0, 10, null);

            // assert - ensure factory received the scheme (verify last string parameter)
            _taskResponseFactoryMock.Verify(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.Is<string>(s => s == "https")), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.Is<string>(s => s == "https")), Times.Once);
        }

        [Test]
        public void GetAll_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var cts = new CancellationToken(true);
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(cts, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Throws(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.GetAll(cts, null, 0, 10, null));
        }

        [Test]
        public void GetAll_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Throws(new InvalidOperationException("service failed"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.GetAll(CancellationToken.None, null, 0, 10, null));
        }

        [Test]
        public async Task GetAll_WhenFactoryReturnsNull_HandlesGracefully()
        {
            // arrange
            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(tasks);

            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns((List<KanbanTaskResponse>?)null);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns((PagedResultKanbanTasksResponse<KanbanTaskResponse>?)null);

            // act
            var action = await _taskController.GetAll(CancellationToken.None, null, 0, 10, null);

            // assert
            var ok = action.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok.Value, Is.Null);
        }

        [Test]
        public async Task GetAll_UrlHelperPassedToFactory()
        {
            // arrange
            var urlHelperMock = new Mock<IUrlHelper>();
            _taskController.Url = urlHelperMock.Object;

            var tasks = new List<KanbanTask> { new KanbanTask { Id = 1 } };
            _taskServiceMock.Setup(s => s.GetPaginatedTasksAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .ReturnsAsync(tasks);

            var responses = tasks.Select(t => new KanbanTaskResponse { Id = t.Id }).ToList();
            _taskResponseFactoryMock.Setup(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(responses);
            _taskResponseFactoryMock.Setup(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(new PagedResultKanbanTasksResponse<KanbanTaskResponse>(responses, responses.Count, 0, 10));

            // act
            await _taskController.GetAll(CancellationToken.None, null, 0, 10, null);

            // assert
            _taskResponseFactoryMock.Verify(f => f.CreateListFoundTasksWithHateoas(It.IsAny<List<KanbanTask?>>(), It.Is<IUrlHelper>(u => ReferenceEquals(u, urlHelperMock.Object)), It.IsAny<string>()), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreatePagedResult_WithHateoasLinksFor(It.IsAny<List<KanbanTaskResponse>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.Is<IUrlHelper>(u => ReferenceEquals(u, urlHelperMock.Object)), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region GetById

        [Test]
        public async Task GetById_TaskExists_ReturnsOkWithTaskDto()
        {
            // arrange
            var id = 42;
            var foundTask = new KanbanTask { Id = id, Name = "Task 42" };
            var expectedDto = new KanbanTaskResponse { Id = id, Name = "Task 42", Links = new List<LinkDTO>() };

            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(foundTask);

            _taskResponseFactoryMock.Setup(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedDto);

            // act
            var action = await _taskController.GetById(id, CancellationToken.None);

            // assert
            var ok = action.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(expectedDto));

            _taskServiceMock.Verify(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetById_TaskNotFound_ThrowsKeyNotFoundException()
        {
            // arrange
            var id = 99;
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((KanbanTask?)null);

            // act & assert
            Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(async () => await _taskController.GetById(id, CancellationToken.None));
            _taskServiceMock.Verify(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetById_UsesRequestSchemeWhenBuildingLinks()
        {
            // arrange
            var id = 1;
            var foundTask = new KanbanTask { Id = id };
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(foundTask);

            var expectedDto = new KanbanTaskResponse { Id = id, Links = new List<LinkDTO>() };
            _taskResponseFactoryMock.Setup(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedDto);

            // change scheme to https
            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            _taskController.ControllerContext = new ControllerContext { HttpContext = ctx };

            // act
            await _taskController.GetById(id, CancellationToken.None);

            // assert - verify factory received the scheme
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.IsAny<IUrlHelper>(), It.Is<string>(s => s == "https")), Times.Once);
        }

        [Test]
        public async Task GetById_UrlHelperPassedToFactory()
        {
            // arrange
            var id = 5;
            var foundTask = new KanbanTask { Id = id };
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(foundTask);

            var urlHelperMock = new Mock<IUrlHelper>();
            _taskController.Url = urlHelperMock.Object;

            var expectedDto = new KanbanTaskResponse { Id = id, Links = new List<LinkDTO>() };
            _taskResponseFactoryMock.Setup(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedDto);

            // act
            await _taskController.GetById(id, CancellationToken.None);

            // assert - ensure same Url instance forwarded
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(id, foundTask, It.Is<IUrlHelper>(u => ReferenceEquals(u, urlHelperMock.Object)), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetById_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            var id = 7;
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service failure"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.GetById(id, CancellationToken.None));
        }

        [Test]
        public void GetById_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var id = 3;
            var cts = new CancellationToken(true); // already cancelled
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(id, cts))
                .ThrowsAsync(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.GetById(id, cts));
        }
        #endregion

        #region Create

        [Test]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            // arrange
            var createRequest = new CreateKanbanTaskRequest
            {
                Name = "New Task",
                Description = "desc",
                Status = StatusEnum.ToDo
            };

            var createdTask = new KanbanTask { Id = 123, Name = createRequest.Name };
            var expectedDto = new KanbanTaskResponse { Id = createdTask.Id, Name = createdTask.Name, Links = new List<LinkDTO>() };

            _taskServiceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTask);

            _taskResponseFactoryMock.Setup(f => f.CreateFoundTaskWithHateoas(createdTask.Id, createdTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedDto);

            // act
            var action = await _taskController.Create(createRequest, CancellationToken.None);

            // assert
            var created = action as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(created.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(created.ActionName, Is.EqualTo(nameof(TasksController.GetById)));
            Assert.That(created.RouteValues, Is.Not.Null);
            Assert.That(created.RouteValues["id"], Is.EqualTo(expectedDto.Id));
            Assert.That(created.Value, Is.SameAs(expectedDto));

            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.Is<CreateKanbanTaskRequest>(r => ReferenceEquals(r, createRequest)), It.IsAny<CancellationToken>()), Times.Once);
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(createdTask.Id, createdTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Create_InvalidModelState_ReturnsBadRequest_AndDoesNotCallService()
        {
            // arrange
            _taskController.ModelState.AddModelError("Name", "Required");
            var createRequest = new CreateKanbanTaskRequest();

            // act
            var action = await _taskController.Create(createRequest, CancellationToken.None);

            // assert
            var bad = action as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(It.IsAny<int>(), It.IsAny<KanbanTask>(), It.IsAny<IUrlHelper>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Create_UsesRequestSchemeAndUrlHelper()
        {
            // arrange
            var createRequest = new CreateKanbanTaskRequest { Name = "x" };
            var createdTask = new KanbanTask { Id = 7, Name = "x" };
            var expectedDto = new KanbanTaskResponse { Id = 7, Links = new List<LinkDTO>() };

            _taskServiceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTask);

            _taskResponseFactoryMock.Setup(f => f.CreateFoundTaskWithHateoas(createdTask.Id, createdTask, It.IsAny<IUrlHelper>(), It.IsAny<string>()))
                .Returns(expectedDto);

            var ctx = new DefaultHttpContext();
            ctx.Request.Scheme = "https";
            _taskController.ControllerContext = new ControllerContext { HttpContext = ctx };

            var urlHelperMock = new Mock<IUrlHelper>();
            _taskController.Url = urlHelperMock.Object;

            // act
            await _taskController.Create(createRequest, CancellationToken.None);

            // assert
            _taskResponseFactoryMock.Verify(f => f.CreateFoundTaskWithHateoas(createdTask.Id, createdTask, It.Is<IUrlHelper>(u => ReferenceEquals(u, urlHelperMock.Object)), It.Is<string>(s => s == "https")), Times.Once);
        }

        [Test]
        public void Create_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            var createRequest = new CreateKanbanTaskRequest { Name = "x" };
            _taskServiceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service failed"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.Create(createRequest, CancellationToken.None));
        }

        [Test]
        public void Create_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var createRequest = new CreateKanbanTaskRequest { Name = "x" };
            var cts = new CancellationToken(true);
            _taskServiceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), cts))
                .ThrowsAsync(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.Create(createRequest, cts));
        }
        #endregion

        #region EditFullUpdate
        [Test]
        public async Task EditFullUpdate_ValidRequest_ReturnsNoContent_AndCallsService()
        {
            // arrange
            var id = 10;
            var dto = new FullUpdateKanbanTaskRequest { Name = "Updated", Description = "d" };
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // act
            var action = await _taskController.EditFullUpdate(id, dto, CancellationToken.None);

            // assert
            var noContent = action as NoContentResult;
            Assert.That(noContent, Is.Not.Null);
            Assert.That(noContent.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(id, It.Is<FullUpdateKanbanTaskRequest>(r => ReferenceEquals(r, dto)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task EditFullUpdate_InvalidModelState_ReturnsBadRequest_AndDoesNotCallService()
        {
            // arrange
            _taskController.ModelState.AddModelError("Name", "Required");
            var dto = new FullUpdateKanbanTaskRequest();

            // act
            var action = await _taskController.EditFullUpdate(1, dto, CancellationToken.None);

            // assert
            var bad = action as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void EditFullUpdate_TaskNotFound_ThrowsKeyNotFoundException()
        {
            // arrange
            var id = 99;
            var dto = new FullUpdateKanbanTaskRequest { Name = "x" };
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // act & assert
            Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(async () => await _taskController.EditFullUpdate(id, dto, CancellationToken.None));
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(id, It.Is<FullUpdateKanbanTaskRequest>(r => ReferenceEquals(r, dto)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void EditFullUpdate_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            var id = 7;
            var dto = new FullUpdateKanbanTaskRequest { Name = "x" };
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service failed"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.EditFullUpdate(id, dto, CancellationToken.None));
        }

        [Test]
        public void EditFullUpdate_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var id = 3;
            var dto = new FullUpdateKanbanTaskRequest { Name = "x" };
            var cts = new CancellationToken(true); // already cancelled
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(id, dto, cts))
                .ThrowsAsync(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.EditFullUpdate(id, dto, cts));
        }
        #endregion

        #region EditPartialUpdate

        [Test]
        public async Task EditPartialUpdate_ValidRequest_ReturnsNoContent_AndCallsService()
        {
            // arrange
            var id = 11;
            var dto = new PartialUpdateKanbanTaskRequest { Description = "Partial update" };
            _taskServiceMock.Setup(s => s.PartialUpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // act
            var action = await _taskController.EditPartialUpdate(id, dto, CancellationToken.None);

            // assert
            var noContent = action as NoContentResult;
            Assert.That(noContent, Is.Not.Null);
            Assert.That(noContent.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            _taskServiceMock.Verify(s => s.PartialUpdateTaskAsync(id, It.Is<PartialUpdateKanbanTaskRequest>(r => ReferenceEquals(r, dto)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task EditPartialUpdate_InvalidModelState_ReturnsBadRequest_AndDoesNotCallService()
        {
            // arrange
            _taskController.ModelState.AddModelError("Description", "Required");
            var dto = new PartialUpdateKanbanTaskRequest();

            // act
            var action = await _taskController.EditPartialUpdate(1, dto, CancellationToken.None);

            // assert
            var bad = action as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            _taskServiceMock.Verify(s => s.PartialUpdateTaskAsync(It.IsAny<int>(), It.IsAny<PartialUpdateKanbanTaskRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void EditPartialUpdate_TaskNotFound_ThrowsKeyNotFoundException()
        {
            // arrange
            var id = 99;
            var dto = new PartialUpdateKanbanTaskRequest { Description = "x" };
            _taskServiceMock.Setup(s => s.PartialUpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // act & assert
            Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(async () => await _taskController.EditPartialUpdate(id, dto, CancellationToken.None));
            _taskServiceMock.Verify(s => s.PartialUpdateTaskAsync(id, It.Is<PartialUpdateKanbanTaskRequest>(r => ReferenceEquals(r, dto)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void EditPartialUpdate_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            var id = 7;
            var dto = new PartialUpdateKanbanTaskRequest { Description = "x" };
            _taskServiceMock.Setup(s => s.PartialUpdateTaskAsync(id, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service failed"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.EditPartialUpdate(id, dto, CancellationToken.None));
        }

        [Test]
        public void EditPartialUpdate_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var id = 3;
            var dto = new PartialUpdateKanbanTaskRequest { Description = "x" };
            var cts = new CancellationToken(true); // already cancelled
            _taskServiceMock.Setup(s => s.PartialUpdateTaskAsync(id, dto, cts))
                .ThrowsAsync(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.EditPartialUpdate(id, dto, cts));
        }

        #endregion

        #region Delete

        [Test]
        public async Task Delete_ValidRequest_ReturnsNoContent_AndCallsService()
        {
            // arrange
            var id = 21;
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // act
            var action = await _taskController.Delete(id, CancellationToken.None);

            // assert
            var noContent = action as NoContentResult;
            Assert.That(noContent, Is.Not.Null);
            Assert.That(noContent.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            _taskServiceMock.Verify(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Delete_TaskNotFound_ThrowsKeyNotFoundException()
        {
            // arrange
            var id = 99;
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // act & assert
            var ex = Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(async () => await _taskController.Delete(id, CancellationToken.None));
            Assert.That(ex.Message, Does.Contain(id.ToString()));
            _taskServiceMock.Verify(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Delete_WhenServiceThrows_ExceptionPropagates()
        {
            // arrange
            var id = 7;
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service failed"));

            // act & assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _taskController.Delete(id, CancellationToken.None));
        }

        [Test]
        public void Delete_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // arrange
            var id = 3;
            var cts = new CancellationToken(true); // already cancelled
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(id, cts))
                .ThrowsAsync(new OperationCanceledException());

            // act & assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _taskController.Delete(id, cts));
        }

        #endregion

    }
}

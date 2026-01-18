using AutoMapper;
using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.ModelClasses;
using KanbanRestService.Hubs;
using KanbanRestService.Services;
using Moq;
using NUnit.Framework;

namespace KanbanTests.Unit.Services
{
    [TestFixture]
    internal class TaskServiceTests
    {
        private ITaskService _taskService;
        private Mock<IUnitOfWork<KanbanAppDbContext>> _unitOfWorkMock;
        private Mock<IGenericRepository<KanbanTask>> _repoMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ITaskNotifications> _notifierMock;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<KanbanAppDbContext>>();
            _repoMock = new Mock<IGenericRepository<KanbanTask>>();
            _mapperMock = new Mock<IMapper>();
            _notifierMock = new Mock<ITaskNotifications>();

            _taskService = new TaskServiceHost(_unitOfWorkMock.Object, _repoMock.Object, _notifierMock.Object, _mapperMock.Object);
        }

        #region GetPaginatedTasksAsync
        [Test]
        public void GetPaginatedTasks_GetAll_NoPagination()
        {

            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());
            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, null).Result;

            //assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo("Task 1"));
            Assert.That(result[1].Name, Is.EqualTo("Task 2"));
            Assert.That(result[2].Name, Is.EqualTo("Task 3"));
        }

        [Test]
        public void GetPaginatedTasks_GetByStatus_NoPagination()
        {

            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());
            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, "Completed", 0, 0, null).Result;

            //assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Task 3"));
        }

        [Test]
        public void GetPaginatedTasks_SortById_NoPagination()
        {

            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());
            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, null).Result;

            //assert
            var expectedOrder = new List<int> { 1, 2, 3 };
            Assert.That(result.Select(t => t.Id), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByNameAsc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "Name,asc" };
            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<string> { "Task 1", "Task 2", "Task 3" };
            Assert.That(result.Select(t => t.Name), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByPriorityAsc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.ToDo, PriorityEnum = PriorityEnum.Medium},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.InProgress, PriorityEnum = PriorityEnum.Low},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed, PriorityEnum = PriorityEnum.High }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "priority,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<PriorityEnum> { PriorityEnum.Low, PriorityEnum.Medium, PriorityEnum.High };
            Assert.That(result.Select(t => t.PriorityEnum), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByStatusAsc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed};
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortBySizeAsc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Size = 2},
                    new KanbanTask { Id = 1, Name = "Task 1", Size = 1},
                    new KanbanTask { Id = 3, Name = "Task 3", Size = 3}
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "size,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<int> { 1, 2, 3 };
            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortBySizeAscStatusDesc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 4, Name = "Task 4", Size = 2, Status = StatusEnum.Completed},
                    new KanbanTask { Id = 2, Name = "Task 2", Size = 2, Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Size = 1, Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Size = 3, Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities()).Returns(taskList.AsQueryable());

            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) =>
                    q.Provider.CreateQuery<KanbanTask>(q.Expression).ToList());

            var sortString = new List<string> { "size,asc","status,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var expectedStatusOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.Completed, StatusEnum.InProgress, StatusEnum.Completed};

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedStatusOrder));
        }

        [Test]
        public void GetPaginatedTasks_InvalidStatusInvalidStatusSort_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "TRalala,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, "INVALIDSTATUS", 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<int> { 1, 2, 3 };
            Assert.That(result.Select(t => t.Id), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_StatusCaseInsensitive_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, "CoMpLeTeD", 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.Completed };
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByStatusMissingDirection_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed };
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByStatusUnknownDirection_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status,Bla" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed };
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByStatusCaseInsensitive_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status,AsC" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed };
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByStatusEmptyString_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "", "" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed };
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_EmptySortField_NoPagination()
        {
            //arrange sort field count = 0
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<int> { 1, 2, 3};
            Assert.That(result.Select(t => t.Id), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_EmptyRepository()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {

                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "status,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed };
            Assert.That(result.Any, Is.False);
        }

        [Test]
        public void GetPaginatedTasks_NoSorting_Page0and1()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            //act
            var result0page = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 2, null).Result;
            var result1stpage = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 1, 2, null).Result;
            
            //assert

            var expectedOrder0Page = new List<string> { "Task 1", "Task 2" };
            Assert.That(result0page.Select(t => t.Name), Is.EqualTo(expectedOrder0Page));

            var expectedOrder2ndPage = new List<string> { "Task 3" };
            Assert.That(result1stpage.Select(t => t.Name), Is.EqualTo(expectedOrder2ndPage));

        }


        [Test]
        public void GetPaginatedTasks_NoSorting_NegativePage()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            //act
            var resultPage = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, -1, 10, null).Result;
            
            //assert
            var expectedOrder = new List<string> { "Task 1", "Task 2", "Task 3"};
            Assert.That(resultPage.Select(t => t.Name), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_NoSorting_NegativePageSize()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            //act
            var resultPage = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, -10, null).Result;

            //assert
            var expectedOrder = new List<string> { "Task 1", "Task 2", "Task 3" };
            Assert.That(resultPage.Select(t => t.Name), Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByName_Page0and1()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var sortString = new List<string> { "Name,desc" };

            //act
            var result0page = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 2, sortString).Result;
            var result1stpage = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 1, 2, sortString).Result;

            //assert

            var expectedOrder0Page = new List<string> { "Task 3", "Task 2" };
            Assert.That(result0page.Select(t => t.Name), Is.EqualTo(expectedOrder0Page));

            var expectedOrder2ndPage = new List<string> { "Task 1" };
            Assert.That(result1stpage.Select(t => t.Name), Is.EqualTo(expectedOrder2ndPage));
        }

        [Test]
        public void GetPaginatedTasks_CancelationTokenForwardedToRepo()
        {
            // arrange
            var taskList = new List<KanbanTask>
            {
                new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress },
                new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo },
                new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
            };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());

            CancellationToken capturedToken = default;

            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .Callback((IQueryable<KanbanTask> q, CancellationToken token) => capturedToken = token)
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) => q.ToList());

            var cts = new CancellationTokenSource();

            // act
            var result = _taskService.GetPaginatedTasksAsync(cts.Token, null, 0, 0, null).Result;

            // assert - verify repository was called with the same token and callback captured it
            _repoMock.Verify(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(),
                It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);

            Assert.That(capturedToken, Is.EqualTo(cts.Token));
        }

        [Test]
        public void GetPaginatedTasks_ExceptionThrown()
        {
            // arrange
            var taskList = new List<KanbanTask>
            {
                new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress },
                new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo },
                new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
            };

            _repoMock.Setup(x => x.GetQueryableEntities())
                .Returns(taskList.AsQueryable());

            // Make repository throw when attempting to materialize the query
            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Repository failure"));

            // act + assert: unwrap the task exception with GetAwaiter().GetResult() so Assert.Throws sees the original exception
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, null).GetAwaiter().GetResult());
        }

        #endregion

        #region GetTaskByIdAsync
        [Test]
        public void GetTaskById_ValidId_Successfull()
        {

            //arrange
            var expectedTask = new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo};
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    expectedTask,
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                    taskList.FirstOrDefault(t => t.Id == id)
                );

            //act
            var resultTask = _taskService.GetTaskByIdAsync(expectedTask.Id, CancellationToken.None).Result;

            //assert
            
            Assert.That(resultTask.Id, Is.EqualTo(expectedTask.Id));
            Assert.That(resultTask.Name, Is.EqualTo(expectedTask.Name));
        }


        [Test]
        public void GetTaskById_NonexistentId_NoTaskReturned()
        {

            //arrange
            var expectedTask = new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo };
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    expectedTask,
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };

            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                    taskList.FirstOrDefault(t => t.Id == id)
                );

            //act
            var resultTask = _taskService.GetTaskByIdAsync(56, CancellationToken.None).Result;

            //assert
            Assert.That(resultTask, Is.Null);            
        }

        [Test]
        public void GetTaskById_CancelationTokenPropagated()
        {

            //arrange
            var expectedTask = new KanbanTask { Id = 1, Name = "Task 1", Status = StatusEnum.ToDo };
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 2, Name = "Task 2", Status = StatusEnum.InProgress},
                    expectedTask,
                    new KanbanTask { Id = 3, Name = "Task 3", Status = StatusEnum.Completed }
                };


            CancellationToken capturedToken = default;
            var cts = new CancellationTokenSource();

            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                    {
                        capturedToken = ct;
                        return taskList.FirstOrDefault(t => t.Id == id);
                    }
                );

            //act
            var resultTask = _taskService.GetTaskByIdAsync(56, cts.Token).Result;

            //assert
            _repoMock.Verify(x => x.FindAsync(It.IsAny<int>(), 
                It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);

            Assert.That(capturedToken, Is.EqualTo(cts.Token));
        }

        [Test]
        public void GetTaskById_VerifyRepositoryCalledWithCorrectId()
        {
            // arrange
            var expectedTask = new KanbanTask { Id = 42, Name = "Answer", Status = StatusEnum.ToDo };

            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => expectedTask.Id == id ? expectedTask : null);

            // act
            var result = _taskService.GetTaskByIdAsync(expectedTask.Id, CancellationToken.None).Result;

            // assert
            _repoMock.Verify(x => x.FindAsync(expectedTask.Id, It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedTask.Id));
        }

        [Test]
        public void GetTaskById_RepositoryThrows_ExceptionPropagates()
        {
            // arrange
            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("repo failure"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.GetTaskByIdAsync(1, CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void GetTaskById_CanceledToken_ThrowsOperationCanceledException()
        {
            // arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // make repository return a canceled task when given the token
            _repoMock.Setup(x => x.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((int id, CancellationToken ct) => Task.FromCanceled<KanbanTask>(ct));

            // act + assert
            Assert.Throws<TaskCanceledException>(() =>
                _taskService.GetTaskByIdAsync(1, cts.Token).GetAwaiter().GetResult());
        }

        #endregion

        #region CreateTaskAsync

        [Test]
        public void CreateTask_WithNullDescription_SetsDescriptionToEmptyString()
        {

            Assert.Pass("This is a placeholder test.");
        }

        #endregion

        #region PartialUpdateTaskAsync
        [Test]
        public void PartialUpdateTask_x_x()
        {

            Assert.Pass("This is a placeholder test.");
        }
        #endregion

        #region UpdateTaskAsync
        [Test]
        public void UpdateTask_x_x()
        {

            Assert.Pass("This is a placeholder test.");
        }
        #endregion

        #region DeleteTaskAsync
        [Test]
        public void DeleteTask_x_x()
        {

            Assert.Pass("This is a placeholder test.");
        }
        #endregion


    }
}

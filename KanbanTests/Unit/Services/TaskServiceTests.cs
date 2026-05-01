using AutoMapper;
using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.DTOs.RequestDTOs;
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

        #region Constructor validation
        [Test]
        public void TaskService_Constructor_UnitOfWorkNullParameter_ThrowsArgumentNullException()
        {         
            Assert.Throws<ArgumentNullException>(() => new TaskServiceHost(null, _repoMock.Object, _notifierMock.Object, _mapperMock.Object));
        }

        [Test]
        public void TaskService_Constructor_RepoNullParameter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskServiceHost(_unitOfWorkMock.Object, null, _notifierMock.Object, _mapperMock.Object));
        }

        [Test]
        public void TaskService_Constructor_MapperNullParameter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskServiceHost(_unitOfWorkMock.Object, _repoMock.Object, _notifierMock.Object, null));
        }

        [Test]
        public void TaskService_Constructor_NotifierParameter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskServiceHost(_unitOfWorkMock.Object, _repoMock.Object, null, _mapperMock.Object));
        }
        #endregion

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
        public void GetPaginatedTasks_SortByPriorityDesc_NoPagination()
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

            var sortString = new List<string> { "priority,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<PriorityEnum> { PriorityEnum.High, PriorityEnum.Medium, PriorityEnum.Low };
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
        public void GetPaginatedTasks_SortByStatusDesc_NoPagination()
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

            var sortString = new List<string> { "status,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<StatusEnum> { StatusEnum.Completed, StatusEnum.InProgress, StatusEnum.ToDo };
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
        public void GetPaginatedTasks_SortBySizeDesc_NoPagination()
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

            var sortString = new List<string> { "size,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedOrder = new List<int> { 3, 2, 1 };
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
        public void GetPaginatedTasks_SortBySizeAscStatusAsc_NoPagination()
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

            var sortString = new List<string> { "size,asc", "status,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var expectedStatusOrder = new List<StatusEnum> { StatusEnum.ToDo, StatusEnum.InProgress, StatusEnum.Completed, StatusEnum.Completed };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Status), Is.EqualTo(expectedStatusOrder));
        }

        public void GetPaginatedTasks_SortBySizeAscNameAsc_NoPagination()
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

            var sortString = new List<string> { "size,asc", "name,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var expectedNameOrder = new List<string> { "Task 1", "Task 2", "Task 3", "Task 4" };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(expectedNameOrder));
        }

        public void GetPaginatedTasks_SortBySizeAscNameDesc_NoPagination()
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

            var sortString = new List<string> { "size,asc", "name,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var expectedNameOrder = new List<string> { "Task 4", "Task 3", "Task 2", "Task 1" };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(expectedNameOrder));
        }

        public void GetPaginatedTasks_SortBySizeAscPriorityAsc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 4, Name = "Task 4", Size = 2, PriorityEnum = PriorityEnum.High, Status = StatusEnum.Completed},
                    new KanbanTask { Id = 2, Name = "Task 2", Size = 2, PriorityEnum = PriorityEnum.Medium, Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Size = 1, PriorityEnum = PriorityEnum.Medium, Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Size = 3, PriorityEnum = PriorityEnum.Low, Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities()).Returns(taskList.AsQueryable());

            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) =>
                    q.Provider.CreateQuery<KanbanTask>(q.Expression).ToList());

            var sortString = new List<string> { "size,asc", "priority,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var priorityNameOrder = new List<PriorityEnum> { PriorityEnum.Low, PriorityEnum.Medium, PriorityEnum.Medium, PriorityEnum.High };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(priorityNameOrder));
        }

        public void GetPaginatedTasks_SortBySizeAscPriorityDesc_NoPagination()
        {
            //arrange
            var taskList = new List<KanbanTask>
                {
                    new KanbanTask { Id = 4, Name = "Task 4", Size = 2, PriorityEnum = PriorityEnum.High, Status = StatusEnum.Completed},
                    new KanbanTask { Id = 2, Name = "Task 2", Size = 2, PriorityEnum = PriorityEnum.Medium, Status = StatusEnum.InProgress},
                    new KanbanTask { Id = 1, Name = "Task 1", Size = 1, PriorityEnum = PriorityEnum.Medium, Status = StatusEnum.ToDo},
                    new KanbanTask { Id = 3, Name = "Task 3", Size = 3, PriorityEnum = PriorityEnum.Low, Status = StatusEnum.Completed}
                };

            _repoMock.Setup(x => x.GetQueryableEntities()).Returns(taskList.AsQueryable());

            _repoMock.Setup(x => x.GetEntitiesBasedOn(It.IsAny<IQueryable<KanbanTask>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<KanbanTask> q, CancellationToken _) =>
                    q.Provider.CreateQuery<KanbanTask>(q.Expression).ToList());

            var sortString = new List<string> { "size,asc", "priority,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedSizeOrder = new List<int> { 1, 2, 2, 3 };
            var priorityNameOrder = new List<PriorityEnum> { PriorityEnum.High, PriorityEnum.Medium, PriorityEnum.Medium, PriorityEnum.Low };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(priorityNameOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByNameAscSizeAsc_NoPagination()
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

            var sortString = new List<string> { "name,asc", "size,asc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedNameOrder = new List<string> { "Task 1", "Task 2", "Task 3", "Task 4" };
            var expectedSizeOrder = new List<int> { 1, 2, 3, 2 };            

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(expectedNameOrder));
        }

        [Test]
        public void GetPaginatedTasks_SortByNameAscSizeDesc_NoPagination()
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

            var sortString = new List<string> { "name,asc", "size,desc" };

            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, sortString).Result;

            //assert
            var expectedNameOrder = new List<string> { "Task 1", "Task 2", "Task 3", "Task 4" };
            var expectedSizeOrder = new List<int> { 1, 2, 3, 2 };

            Assert.That(result.Select(t => t.Size), Is.EqualTo(expectedSizeOrder));
            Assert.That(result.Select(t => t.Name), Is.EqualTo(expectedNameOrder));
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
        public void CreateTask_ValidRequest_AllMethodsAreCalledOnceWithValidData()
        {
            //arrange
            var createdTask = new CreateKanbanTaskRequest
            {
                Name = "New Task",
                Description = "test description",
                Status = StatusEnum.ToDo,
                PriorityEnum = PriorityEnum.Medium,
                Size = 2
            };

            _mapperMock.Setup(x => x.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns((CreateKanbanTaskRequest req) => new KanbanTask
                {
                    Id = 1,
                    Name = req.Name,
                    Description = req.Description ?? string.Empty,
                    Status = req.Status,
                    PriorityEnum = req.PriorityEnum,
                    Size = req.Size
                });

            _repoMock.Setup(x => x.Add(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(x => x.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            //act
            var resultTask = _taskService.CreateTaskAsync(createdTask, CancellationToken.None).Result;

            //assert
            Assert.That(resultTask.Name, Is.EqualTo(createdTask.Name));
            Assert.That(resultTask.Description, Is.EqualTo(createdTask.Description));
            Assert.That(resultTask.Size, Is.EqualTo(createdTask.Size));
            Assert.That(resultTask.Status, Is.EqualTo(createdTask.Status));
            Assert.That(resultTask.PriorityEnum, Is.EqualTo(createdTask.PriorityEnum));

            _mapperMock.Verify(x => x.Map<KanbanTask>(It.Is<CreateKanbanTaskRequest>(r => r == createdTask)), Times.Once);
            _repoMock.Verify(x => x.Add(It.Is<KanbanTask>(t =>
                t.Name == createdTask.Name &&
                t.Description == createdTask.Description &&
                t.PriorityEnum == createdTask.PriorityEnum &&
                t.Size == createdTask.Size &&
                t.Status == createdTask.Status)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(It.Is<CancellationToken>(c => c == CancellationToken.None)), Times.Once);
            _notifierMock.Verify(x => x.TaskCreated(It.Is<KanbanTask>(t =>
                t.Name == createdTask.Name &&
                t.Description == createdTask.Description &&
                t.Status == createdTask.Status &&
                t.PriorityEnum == createdTask.PriorityEnum &&
                t.Size == createdTask.Size
            ), It.Is<CancellationToken>(c => c == CancellationToken.None)), Times.Once);
        }

        [Test]
        public void CreateTask_TaskRequestIsNull_AllMethodsAreCalledOnceWithValidData()
        {
            //arrange
            CreateKanbanTaskRequest expectedTask = null;

            //act + assert
            Assert.Throws<ArgumentNullException>(() => _taskService.CreateTaskAsync(expectedTask, CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void CreateTask_TaskRequestValid_CancelationTokenPropagatedToAll()
        {
            //arrange
            var createdTask = new CreateKanbanTaskRequest
            {
                Name = "New Task",
                Description = "test description",
                Status = StatusEnum.ToDo,
                PriorityEnum = PriorityEnum.Medium,
                Size = 2
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken resultTokenNotifier = default;
            CancellationToken resultTokenUnitOfWork = default;
            
            _mapperMock.Setup(x => x.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns((CreateKanbanTaskRequest req) => new KanbanTask
                {
                    Id = 1,
                    Name = req.Name,
                    Description = req.Description ?? string.Empty,
                    Status = req.Status,
                    PriorityEnum = req.PriorityEnum,
                    Size = req.Size
                });

            _notifierMock.Setup(x => x.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Callback((KanbanTask t, CancellationToken ct) =>
                {
                    resultTokenNotifier = ct;
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Callback((CancellationToken ct) =>
                {
                    resultTokenUnitOfWork = ct;
                })
                .Returns(Task.CompletedTask);
            //act
            var result = _taskService.CreateTaskAsync(createdTask, cts.Token).Result;

            //assert
            Assert.That(resultTokenNotifier, Is.EqualTo(cts.Token));
            Assert.That(resultTokenUnitOfWork, Is.EqualTo(cts.Token));
        }

        [Test]
        public void CreateTask_MapperReturnsNull_CurrentBehavior_ReturnsNullAndNotifierReceivesNull()
        {
            //arrange
            var createdTask = new CreateKanbanTaskRequest
            {
                Name = "Null mapped"
            };


            _mapperMock.Setup(x => x.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                 .Returns((KanbanTask?)null);

            _repoMock.Setup(x => x.Add(null)).Verifiable();

            _unitOfWorkMock.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            KanbanTask? notifiedTask = new();
            _notifierMock.Setup(x => x.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Callback((KanbanTask t, CancellationToken ct) =>
                {
                    notifiedTask = t;
                })
                .Returns(Task.CompletedTask);

            //act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(createdTask, CancellationToken.None).GetAwaiter().GetResult());
                    
            
            _repoMock.Verify(x => x.Add(null), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(x => x.TaskCreated(null, It.IsAny<CancellationToken>()), Times.Never);
            
        }

        [Test]
        public void CreateTask_SaveAsyncThrows_NotifierNotCalledAndExceptionPropagates()
        {
            // arrange
            var createdTask = new CreateKanbanTaskRequest
            {
                Name = "New Task",
                Description = "desc",
                Status = StatusEnum.ToDo,
                PriorityEnum = PriorityEnum.Medium,
                Size = 1
            };

            var mapped = new KanbanTask { Id = 1, Name = createdTask.Name };

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns(mapped);

            _repoMock.Setup(r => r.Add(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("save failure"));

            _notifierMock.Setup(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(createdTask, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Add(It.Is<KanbanTask>(t => t == mapped)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateTask_RepoAddThrows_NoSaveOrNotifyCalled()
        {
            // arrange
            var createdTask = new CreateKanbanTaskRequest { Name = "New Task" };
            var mapped = new KanbanTask { Id = 1, Name = createdTask.Name };

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns(mapped);

            _repoMock.Setup(r => r.Add(It.IsAny<KanbanTask>()))
                .Throws(new InvalidOperationException("add failure"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(createdTask, CancellationToken.None).GetAwaiter().GetResult());

            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateTask_NotifierThrows_ExceptionPropagatesButSaveAlreadyCalled()
        {
            // arrange
            var createdTask = new CreateKanbanTaskRequest { Name = "New Task" };
            var mapped = new KanbanTask { Id = 1, Name = createdTask.Name };

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns(mapped);

            _repoMock.Setup(r => r.Add(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("notify failure"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(createdTask, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Add(It.Is<KanbanTask>(t => t == mapped)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateTask_AddThenSaveThenNotify_OrderIsRespected()
        {
            // arrange
            var createdTask = new CreateKanbanTaskRequest { Name = "Ordered Task" };
            var mapped = new KanbanTask { Id = 1, Name = createdTask.Name };

            var seq = new MockSequence();

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<CreateKanbanTaskRequest>()))
                .Returns(mapped);

            _repoMock.InSequence(seq).Setup(r => r.Add(It.IsAny<KanbanTask>())).Verifiable();
            _unitOfWorkMock.InSequence(seq).Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _notifierMock.InSequence(seq).Setup(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // act
            var result = _taskService.CreateTaskAsync(createdTask, CancellationToken.None).Result;

            // assert
            Assert.That(result, Is.Not.Null);
            _repoMock.Verify(r => r.Add(It.Is<KanbanTask>(t => t == mapped)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region PartialUpdateTaskAsync
        [Test]
        public void PartialUpdateTask_IdIsZero_ArgumentException()
        {
            // arrange
            int inputId = 0;
            var updateTask = new PartialUpdateKanbanTaskRequest { Name = "partial update Task" };
            var mapped = new KanbanTask { Id = 1, Name = updateTask.Name };

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<PartialUpdateKanbanTaskRequest>()))
               .Returns(mapped);

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<ArgumentException>(() =>
                _taskService.PartialUpdateTaskAsync(inputId, updateTask, CancellationToken.None).GetAwaiter().GetResult());

            // assert
            _repoMock.Verify(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.Update(It.IsAny<KanbanTask>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        
        [Test]
        public void PartialUpdateTask_TaskRequestIsNull_ArgumentException()
        {
            // arrange
            int inputId = 0;
            var updateTask = new PartialUpdateKanbanTaskRequest { Name = "partial update Task" };
            var mapped = new KanbanTask { Id = 1, Name = updateTask.Name };

            _mapperMock.Setup(m => m.Map<KanbanTask>(It.IsAny<PartialUpdateKanbanTaskRequest>()))
               .Returns(mapped);

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<ArgumentException>(() =>
                _taskService.PartialUpdateTaskAsync(inputId, null, CancellationToken.None).GetAwaiter().GetResult());

            // assert
            _repoMock.Verify(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.Update(It.IsAny<KanbanTask>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskCreated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void PartialUpdateTask_TaskNotFound_ReturnsFalseAndDoesNotCallUpdateSaveOrNotify()
        {
            // arrange
            var request = new PartialUpdateKanbanTaskRequest { Name = "updated" };

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((KanbanTask?)null);

            // act
            var result = _taskService.PartialUpdateTaskAsync(123, request, CancellationToken.None).Result;

            // assert
            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.Update(It.IsAny<KanbanTask>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void PartialUpdateTask_TaskFound_MapsUpdatesAndSavesAndNotifies_ReturnsTrue()
        {
            // arrange
            var found = new KanbanTask { Id = 5, Name = "old", Description = "olddesc" };
            var request = new PartialUpdateKanbanTaskRequest { Name = "newname" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            // make mapper apply the update to the found instance and return it
            _mapperMock.Setup(m => m.Map(It.IsAny<PartialUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Callback<PartialUpdateKanbanTaskRequest, KanbanTask>((req, dest) =>
                {
                    if (req.Name != null) dest.Name = req.Name;
                    if (req.Description != null) dest.Description = req.Description;
                })
                .Returns<PartialUpdateKanbanTaskRequest, KanbanTask>((req, dest) => dest);

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            // act
            var result = _taskService.PartialUpdateTaskAsync(found.Id, request, CancellationToken.None).Result;

            // assert
            Assert.That(result, Is.True);
            Assert.That(found.Name, Is.EqualTo("newname")); // mapper applied changes
            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(It.Is<KanbanTask>(t => t == found), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void PartialUpdateTask_FindThrows_ExceptionPropagates()
        {
            // arrange
            var request = new PartialUpdateKanbanTaskRequest { Name = "x" };

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("find failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.PartialUpdateTaskAsync(1, request, CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void PartialUpdateTask_SaveAsyncThrows_NotifierNotCalledAndExceptionPropagates()
        {
            // arrange
            var found = new KanbanTask { Id = 6, Name = "toUpdate" };
            var request = new PartialUpdateKanbanTaskRequest { Name = "updated" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<PartialUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<PartialUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("save failed"));

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.PartialUpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void PartialUpdateTask_NotifierThrows_ExceptionPropagates_ButSaveAlreadyCalled()
        {
            // arrange
            var found = new KanbanTask { Id = 8, Name = "toNotify" };
            var request = new PartialUpdateKanbanTaskRequest { Name = "updated" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<PartialUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<PartialUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();
            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            _notifierMock.Setup(n => n.TaskUpdated(found, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("notify failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.PartialUpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(found, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void PartialUpdateTask_CancellationTokenForwardedToFindSaveAndNotify()
        {
            // arrange
            var found = new KanbanTask { Id = 10, Name = "token" };
            var request = new PartialUpdateKanbanTaskRequest { Name = "tok" };

            CancellationToken capturedFind = default;
            CancellationToken capturedSave = default;
            CancellationToken capturedNotify = default;
            var cts = new CancellationTokenSource();

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                {
                    capturedFind = ct;
                    return found;
                });

            _mapperMock.Setup(m => m.Map(It.IsAny<PartialUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<PartialUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Callback((CancellationToken ct) => capturedSave = ct)
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Callback((KanbanTask t, CancellationToken ct) => capturedNotify = ct)
                .Returns(Task.CompletedTask);

            // act
            var result = _taskService.PartialUpdateTaskAsync(found.Id, request, cts.Token).Result;

            // assert
            Assert.That(result, Is.True);
            Assert.That(capturedFind, Is.EqualTo(cts.Token));
            Assert.That(capturedSave, Is.EqualTo(cts.Token));
            Assert.That(capturedNotify, Is.EqualTo(cts.Token));
        }
        #endregion

        #region UpdateTaskAsync
        
        [Test]
        public void UpdateTask_IdZero_ThrowsArgumentException()
        {
            // arrange
            var request = new FullUpdateKanbanTaskRequest { Name = "x" };

            // act + assert
            Assert.Throws<ArgumentException>(() =>
                _taskService.UpdateTaskAsync(0, request, CancellationToken.None).GetAwaiter().GetResult());

            // verify no repository calls
            _repoMock.Verify(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_NullRequest_ThrowsArgumentException()
        {
            // arrange
            FullUpdateKanbanTaskRequest request = null;

            // act + assert
            Assert.Throws<ArgumentException>(() =>
                _taskService.UpdateTaskAsync(1, request, CancellationToken.None).GetAwaiter().GetResult());

            // verify find not called
            _repoMock.Verify(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_TaskNotFound_ReturnsFalseAndDoesNotCallUpdateSaveOrNotify()
        {
            // arrange
            var request = new FullUpdateKanbanTaskRequest { Name = "new" };

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((KanbanTask?)null);

            // act
            var result = _taskService.UpdateTaskAsync(99, request, CancellationToken.None).Result;

            // assert
            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.Update(It.IsAny<KanbanTask>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_ValidRequest_MapsUpdatesSavesAndNotifies_ReturnsTrue()
        {
            // arrange
            var found = new KanbanTask
            {
                Id = 21,
                Name = "OldName",
                Description = "OldDesc",
                Size = 1,
                PriorityEnum = PriorityEnum.Low,
                Status = StatusEnum.ToDo
            };

            var updateRequest = new FullUpdateKanbanTaskRequest
            {
                Name = "NewName",
                Description = "NewDesc",
                Size = 3,
                PriorityEnum = PriorityEnum.High,
                Status = StatusEnum.Completed
            };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Callback<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) =>
                {
                    dest.Name = req.Name;
                    dest.Description = req.Description;
                    dest.Size = req.Size;
                    dest.PriorityEnum = req.PriorityEnum;
                    dest.Status = req.Status;
                })
                .Returns<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) => dest);

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();
            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var cts = new CancellationTokenSource();

            // act
            var result = _taskService.UpdateTaskAsync(found.Id, updateRequest, cts.Token).Result;

            // assert
            Assert.That(result, Is.True);
            Assert.That(found.Name, Is.EqualTo(updateRequest.Name));
            Assert.That(found.Description, Is.EqualTo(updateRequest.Description));
            Assert.That(found.Size, Is.EqualTo(updateRequest.Size));
            Assert.That(found.PriorityEnum, Is.EqualTo(updateRequest.PriorityEnum));
            Assert.That(found.Status, Is.EqualTo(updateRequest.Status));

            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(It.Is<KanbanTask>(t => t == found), It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
            _mapperMock.Verify(m => m.Map(updateRequest, found), Times.Once);
        }

        [Test]
        public void UpdateTask_FindThrows_ExceptionPropagates()
        {
            // arrange
            var request = new FullUpdateKanbanTaskRequest { Name = "x" };

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("find failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(1, request, CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void UpdateTask_MapperThrows_ExceptionPropagates_NoSaveOrNotifyCalled()
        {
            // arrange
            var found = new KanbanTask { Id = 31, Name = "old" };
            var request = new FullUpdateKanbanTaskRequest { Name = "new" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Throws(new InvalidOperationException("mapper failure"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_RepoUpdateThrows_ExceptionPropagates_NoSaveOrNotifyCalled()
        {
            // arrange
            var found = new KanbanTask { Id = 41, Name = "old" };
            var request = new FullUpdateKanbanTaskRequest { Name = "new" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) => dest);

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>()))
                .Throws(new InvalidOperationException("update failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_SaveAsyncThrows_NotifierNotCalledAndExceptionPropagates()
        {
            // arrange
            var found = new KanbanTask { Id = 51, Name = "toUpdate" };
            var request = new FullUpdateKanbanTaskRequest { Name = "updated" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("save failed"));

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateTask_NotifierThrows_ExceptionPropagates_ButSaveAlreadyCalled()
        {
            // arrange
            var found = new KanbanTask { Id = 61, Name = "toNotify" };
            var request = new FullUpdateKanbanTaskRequest { Name = "updated" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _repoMock.Setup(r => r.Update(It.IsAny<KanbanTask>())).Verifiable();
            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            _notifierMock.Setup(n => n.TaskUpdated(found, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("notify failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(found.Id, request, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Update(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskUpdated(found, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateTask_CancellationTokenForwardedToFindSaveAndNotify()
        {
            // arrange
            var found = new KanbanTask { Id = 71, Name = "token" };
            var request = new FullUpdateKanbanTaskRequest { Name = "tok" };

            CancellationToken capturedFind = default;
            CancellationToken capturedSave = default;
            CancellationToken capturedNotify = default;
            var cts = new CancellationTokenSource();

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                {
                    capturedFind = ct;
                    return found;
                });

            _mapperMock.Setup(m => m.Map(It.IsAny<FullUpdateKanbanTaskRequest>(), It.IsAny<KanbanTask>()))
                .Returns<FullUpdateKanbanTaskRequest, KanbanTask>((req, dest) => { dest.Name = req.Name ?? dest.Name; return dest; });

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Callback((CancellationToken ct) => capturedSave = ct)
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskUpdated(It.IsAny<KanbanTask>(), It.IsAny<CancellationToken>()))
                .Callback((KanbanTask t, CancellationToken ct) => capturedNotify = ct)
                .Returns(Task.CompletedTask);

            // act
            var result = _taskService.UpdateTaskAsync(found.Id, request, cts.Token).Result;

            // assert
            Assert.That(result, Is.True);
            Assert.That(capturedFind, Is.EqualTo(cts.Token));
            Assert.That(capturedSave, Is.EqualTo(cts.Token));
            Assert.That(capturedNotify, Is.EqualTo(cts.Token));
        }
        #endregion

        #region DeleteTaskAsync

        [Test]
        public void DeleteTask_IdZero_ThrowsArgumentException()
        {
            // act + assert
            Assert.Throws<ArgumentException>(() =>
                _taskService.DeleteTaskAsync(0, CancellationToken.None).GetAwaiter().GetResult());
        }

        [Test]
        public void DeleteTask_TaskNotFound_ReturnsFalseAndDoesNotCallRepoOrNotify()
        {
            // arrange
            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((KanbanTask?)null);

            // act
            var result = _taskService.DeleteTaskAsync(5, CancellationToken.None).Result;

            // assert
            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.Delete(It.IsAny<KanbanTask>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.TaskDeleted(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void DeleteTask_TaskFound_DeletesSavesAndNotifies_ReturnsTrue()
        {
            // arrange
            var foundTask = new KanbanTask { Id = 7, Name = "ToDelete" };

            _repoMock.Setup(r => r.FindAsync(foundTask.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(foundTask);
            _repoMock.Setup(r => r.Delete(It.IsAny<KanbanTask>())).Verifiable();
            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _notifierMock.Setup(n => n.TaskDeleted(foundTask.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var cts = new CancellationTokenSource();

            // act
            var result = _taskService.DeleteTaskAsync(foundTask.Id, cts.Token).Result;

            // assert
            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.Delete(It.Is<KanbanTask>(t => t == foundTask)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
            _notifierMock.Verify(n => n.TaskDeleted(foundTask.Id, It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
        }

        [Test]
        public void DeleteTask_CancellationTokenIsForwardedToFindSaveAndNotify()
        {
            // arrange
            var foundTask = new KanbanTask { Id = 9, Name = "TokenTest" };

            CancellationToken capturedFind = default;
            CancellationToken capturedSave = default;
            CancellationToken capturedNotify = default;
            var cts = new CancellationTokenSource();

            _repoMock.Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                {
                    capturedFind = ct;
                    return foundTask;
                });

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Callback((CancellationToken ct) => capturedSave = ct)
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(n => n.TaskDeleted(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((int id, CancellationToken ct) => capturedNotify = ct)
                .Returns(Task.CompletedTask);

            // act
            var result = _taskService.DeleteTaskAsync(foundTask.Id, cts.Token).Result;

            // assert
            Assert.That(result, Is.True);
            Assert.That(capturedFind, Is.EqualTo(cts.Token));
            Assert.That(capturedSave, Is.EqualTo(cts.Token));
            Assert.That(capturedNotify, Is.EqualTo(cts.Token));
        }

        [Test]
        public void DeleteTask_SaveAsyncThrows_NotifierNotCalledAndExceptionPropagates()
        {
            // arrange
            var found = new KanbanTask { Id = 11, Name = "SaveFail" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);
            _repoMock.Setup(r => r.Delete(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("save failed"));

            _notifierMock.Setup(n => n.TaskDeleted(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.DeleteTaskAsync(found.Id, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Delete(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskDeleted(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void DeleteTask_NotifierThrows_ExceptionPropagates_ButSaveAlreadyCalled()
        {
            // arrange
            var found = new KanbanTask { Id = 13, Name = "NotifyFail" };

            _repoMock.Setup(r => r.FindAsync(found.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(found);
            _repoMock.Setup(r => r.Delete(It.IsAny<KanbanTask>())).Verifiable();

            _unitOfWorkMock.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _notifierMock.Setup(n => n.TaskDeleted(found.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("notify failed"));

            // act + assert
            Assert.Throws<InvalidOperationException>(() =>
                _taskService.DeleteTaskAsync(found.Id, CancellationToken.None).GetAwaiter().GetResult());

            _repoMock.Verify(r => r.Delete(It.Is<KanbanTask>(t => t == found)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _notifierMock.Verify(n => n.TaskDeleted(found.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion


    }
}

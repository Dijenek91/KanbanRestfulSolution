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
                .ReturnsAsync(taskList.ToList());
            //act
            var result = _taskService.GetPaginatedTasksAsync(CancellationToken.None, null, 0, 0, null).Result;

            //assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo("Task 1"));
            Assert.That(result[1].Name, Is.EqualTo("Task 2"));
            Assert.That(result[2].Name, Is.EqualTo("Task 3"));
        }

        #endregion

        #region GetTaskByIdAsync
        [Test]
        public void GetTaskById_x_x()
        {

            Assert.Pass("This is a placeholder test.");
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

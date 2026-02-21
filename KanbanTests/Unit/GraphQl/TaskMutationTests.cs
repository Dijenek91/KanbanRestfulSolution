using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.GraphQL.Mutations;
using KanbanRestService.Services;
using Moq;
using NUnit.Framework;

namespace KanbanTests.Unit.GraphQl
{
    [TestFixture]
    internal class TaskMutationTests
    {
        private Mock<ITaskService> _svcMock = null!;

        [SetUp]
        public void SetUp()
        {
            _svcMock = new Mock<ITaskService>(MockBehavior.Strict);
        }

        [Test]
        public async Task CreateTask_Calls_Service_And_Returns_Task()
        {
            var req = new CreateKanbanTaskRequest { Name = "t" };
            var expected = new KanbanTask { Id = 1, Name = "t" };

            _svcMock.Setup(s => s.CreateTaskAsync(req, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);

            var sut = new TaskMutation();
            var result = await sut.CreateTask(req, _svcMock.Object, CancellationToken.None);

            Assert.That(result, Is.SameAs(expected));
            _svcMock.Verify(s => s.CreateTaskAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateTask_Propagates_Cancellation()
        {
            var req = new CreateKanbanTaskRequest();
            var cts = new CancellationToken(true); // already cancelled

            _svcMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateKanbanTaskRequest>(), cts))
                    .ThrowsAsync(new OperationCanceledException());

            var sut = new TaskMutation();
            Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.CreateTask(req, _svcMock.Object, cts));
        }

        [Test]
        public async Task FullUpdateTask_Calls_Service_And_Returns_Result()
        {
            var update = new FullUpdateKanbanTaskRequest { Name = "x" };
            _svcMock.Setup(s => s.UpdateTaskAsync(5, update, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var sut = new TaskMutation();
            var result = await sut.FullUpdateTask(5, update, _svcMock.Object, CancellationToken.None);

            Assert.That(result, Is.True);
            _svcMock.Verify(s => s.UpdateTaskAsync(5, update, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PartialUpdateTask_Calls_Service_And_Returns_Result()
        {
            var partial = new PartialUpdateKanbanTaskRequest { Description = "d" };
            _svcMock.Setup(s => s.PartialUpdateTaskAsync(7, partial, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            var sut = new TaskMutation();
            var result = await sut.PartialUpdateTask(7, partial, _svcMock.Object, CancellationToken.None);

            Assert.That(result, Is.False);
            _svcMock.Verify(s => s.PartialUpdateTaskAsync(7, partial, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteUpdateTask_Calls_Service_And_Returns_Result()
        {
            _svcMock.Setup(s => s.DeleteTaskAsync(9, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var sut = new TaskMutation();
            var result = await sut.DeleteUpdateTask(9, _svcMock.Object, CancellationToken.None);

            Assert.That(result, Is.True);
            _svcMock.Verify(s => s.DeleteTaskAsync(9, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

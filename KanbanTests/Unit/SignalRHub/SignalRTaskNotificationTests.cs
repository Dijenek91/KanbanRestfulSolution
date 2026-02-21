using KanbanModel.ModelClasses;
using KanbanRestService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace KanbanTests.Unit.SignalRHub
{
    [TestFixture]
    internal class SignalRTaskNotificationTests
    {
        private Mock<IHubContext<TasksHub>> _hubContextMock = null!;
        private Mock<IHubClients> _hubClientsMock = null!;
        private Mock<IClientProxy> _clientProxyMock = null!;
        private SignalRTaskNotification _signalNotification = null!;

        [SetUp]
        public void SetUp()
        {
            _hubContextMock = new Mock<IHubContext<TasksHub>>(MockBehavior.Strict);
            _hubClientsMock = new Mock<IHubClients>(MockBehavior.Strict);
            _clientProxyMock = new Mock<IClientProxy>(MockBehavior.Strict);

            // HubContext.Clients => hubClients
            _hubContextMock.SetupGet(h => h.Clients).Returns(_hubClientsMock.Object);
            // hubClients.All => clientProxy
            _hubClientsMock.SetupGet(c => c.All).Returns(_clientProxyMock.Object);

            _signalNotification = new SignalRTaskNotification(_hubContextMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _hubContextMock.VerifyAll();
            _hubClientsMock.VerifyAll();
            _clientProxyMock.VerifyAll();
        }

        [Test]
        public async Task TaskCreated_Sends_Task_To_AllClients()
        {
            var task = new KanbanTask { Id = 42, Name = "New" };
            var ct = CancellationToken.None;

            _clientProxyMock
                .Setup(p => p.SendCoreAsync(
                    It.Is<string>(m => m == "TaskCreated"),
                    It.Is<object?[]>(args => args != null && args.Length == 1 && ReferenceEquals(args[0], task)),
                    It.Is<CancellationToken>(t => t == ct)))
                .Returns(Task.CompletedTask);

            await _signalNotification.TaskCreated(task, ct);

            _clientProxyMock.Verify(p => p.SendCoreAsync(
                    "TaskCreated",
                    It.Is<object?[]>(args => args != null && args.Length == 1 && ReferenceEquals(args[0], task)),
                    ct),
                Times.Once);
        }

        [Test]
        public async Task TaskUpdated_Sends_Task_To_AllClients()
        {
            var task = new KanbanTask { Id = 100, Name = "Updated" };
            var ct = CancellationToken.None;

            _clientProxyMock
                .Setup(p => p.SendCoreAsync(
                    It.Is<string>(m => m == "TaskUpdated"),
                    It.Is<object?[]>(args => args != null && args.Length == 1 && ReferenceEquals(args[0], task)),
                    It.Is<CancellationToken>(t => t == ct)))
                .Returns(Task.CompletedTask);

            await _signalNotification.TaskUpdated(task, ct);

            _clientProxyMock.Verify(p => p.SendCoreAsync(
                    "TaskUpdated",
                    It.Is<object?[]>(args => args != null && args.Length == 1 && ReferenceEquals(args[0], task)),
                    ct),
                Times.Once);
        }

        [Test]
        public async Task TaskDeleted_Sends_TaskId_To_AllClients()
        {
            var id = 7;
            var ct = CancellationToken.None;

            _clientProxyMock
                .Setup(p => p.SendCoreAsync(
                    It.Is<string>(m => m == "TaskDeleted"),
                    It.Is<object?[]>(args => args != null && args.Length == 1 && args[0] is int && (int)args[0] == id),
                    It.Is<CancellationToken>(t => t == ct)))
                .Returns(Task.CompletedTask);

            await _signalNotification.TaskDeleted(id, ct);

            _clientProxyMock.Verify(p => p.SendCoreAsync(
                    "TaskDeleted",
                    It.Is<object?[]>(args => args != null && args.Length == 1 && args[0] is int && (int)args[0] == id),
                    ct),
                Times.Once);
        }

        [Test]
        public void TaskCreated_Propagates_Exception_From_Hub()
        {
            var task = new KanbanTask { Id = 99, Name = "Err" };
            var ct = CancellationToken.None;

            _clientProxyMock
                .Setup(p => p.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("hub failure"));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _signalNotification.TaskCreated(task, ct));
        }
    }
}

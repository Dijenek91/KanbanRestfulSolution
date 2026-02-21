using KanbanInfrastructure.DAL;
using KanbanModel.ModelClasses;
using KanbanRestService.GraphQL.Queries;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KanbanTests.Unit.GraphQl
{
    [TestFixture]
    internal class TaskQueryTests
    {
        private static KanbanAppDbContext CreateInMemoryContext(string dbName, IEnumerable<KanbanTask>? seed = null)
        {
            var options = new DbContextOptionsBuilder<KanbanAppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new KanbanAppDbContext(options);
            if (seed != null)
            {
                ctx.KanbanTasks.AddRange(seed);
                ctx.SaveChanges();
            }
            return ctx;
        }

        [Test]
        public void GetTasks_ReturnsAllSeededTasks_AsQueryable()
        {
            var tasks = new[]
            {
                new KanbanTask { Id = 1, Name = "T1", Description = string.Empty },
                new KanbanTask { Id = 2, Name = "T2", Description = string.Empty }
            };

            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), tasks);
            var sut = new TaskQuery();

            var queryable = sut.GetTasks(ctx);

            // Execute the IQueryable to materialize results
            var list = queryable.ToList();

            Assert.That(list, Is.Not.Null);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.Select(t => t.Id), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public async System.Threading.Tasks.Task GetTaskById_ReturnsTask_WhenExists()
        {
            var expected = new KanbanTask { Id = 5, Name = "Existing", Description = string.Empty };

            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), new[] { expected });
            var sut = new TaskQuery();

            var result = await sut.GetTaskById(5, ctx, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            Assert.That(result.Name, Is.EqualTo(expected.Name));
        }

        [Test]
        public async System.Threading.Tasks.Task GetTaskById_ReturnsNull_WhenNotFound()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), Array.Empty<KanbanTask>());
            var sut = new TaskQuery();

            var result = await sut.GetTaskById(42, ctx, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetTaskById_PropagatesCancellation()
        {
            var existing = new KanbanTask { Id = 7, Name = "x", Description = string.Empty };
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), new[] { existing });
            var sut = new TaskQuery();

            var cts = new CancellationToken(true); // already cancelled

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await sut.GetTaskById(7, ctx, cts);
            });
        }
    }
}

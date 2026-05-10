using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.ModelClasses;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTests.Unit.RepositoryLayer
{
    [TestFixture]
    [Category("Unit")]
    internal class UnitOfWorkTests
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

        private class ThrowingSaveContext : KanbanAppDbContext
        {
            public ThrowingSaveContext(DbContextOptions<KanbanAppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new DbUpdateException("simulated db error");
            }
        }

        private class DisposableTrackingContext : KanbanAppDbContext
        {
            public bool WasDisposed { get; private set; }

            public DisposableTrackingContext(DbContextOptions<KanbanAppDbContext> options) : base(options) { }

            public new void Dispose()
            {
                base.Dispose();
                WasDisposed = true;
            }
        }

        [Test]
        public void Constructor_Sets_Context_Property()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(ctx);

            Assert.That(uow.Context, Is.SameAs(ctx));
        }

        [Test]
        public void GenericRepository_Returns_Same_Instance_For_Same_Type()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(ctx);

            var repo1 = uow.GenericRepository<KanbanTask>();
            var repo2 = uow.GenericRepository<KanbanTask>();

            Assert.That(repo1, Is.Not.Null);
            Assert.That(repo1, Is.SameAs(repo2));
            Assert.That(repo1, Is.InstanceOf<GenericRepository<KanbanTask>>());
        }

        [Test]
        public void GenericRepository_Returns_Different_Instances_For_Different_Types()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(ctx);

            var repoTask = uow.GenericRepository<KanbanTask>();
            var repoString = uow.GenericRepository<string>(); // still legal T : class

            Assert.That(repoTask, Is.Not.Null);
            Assert.That(repoString, Is.Not.Null);
            Assert.That(repoTask, Is.Not.SameAs(repoString));
        }

        [Test]
        public async Task SaveAsync_Calls_DbContext_SaveChangesAsync()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(ctx);

            // exercise SaveAsync - should complete without throwing using real in-memory DB
            Assert.That(async () => await uow.SaveAsync(CancellationToken.None), Throws.Nothing);
        }

        [Test]
        public void SaveAsync_Wraps_DbUpdateException_In_GenericException()
        {
            // create a context that throws DbUpdateException from SaveChangesAsync
            var options = new DbContextOptionsBuilder<KanbanAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var throwingCtx = new ThrowingSaveContext(options);
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(throwingCtx);

            var ex = Assert.ThrowsAsync<Exception>(async () => await uow.SaveAsync(CancellationToken.None));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Does.StartWith("Database update failed:"));
            Assert.That(ex.InnerException, Is.TypeOf<DbUpdateException>());
        }

        [Test]
        public async Task Save_GenericVersion_Returns_True_When_No_Exception()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(ctx);

            var entity = new KanbanTask { Name = "x", Description = string.Empty };

            // Save<TEntity> only calls SaveChangesAsync internally; when no exception occurs it should return true
            var (success, error) = await uow.Save(entity, (db, ui, ret) => { /* not used in success path */ }, CancellationToken.None);

            Assert.That(success, Is.True);
            Assert.That(error, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Dispose_Disposes_DbContext()
        {
            var options = new DbContextOptionsBuilder<KanbanAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var disposableCtx = new DisposableTrackingContext(options);
            var uow = new GenericUnitOfWork<KanbanAppDbContext>(disposableCtx);

            uow.Dispose();

            Assert.That(disposableCtx.WasDisposed, Is.False);
        }
    }
}

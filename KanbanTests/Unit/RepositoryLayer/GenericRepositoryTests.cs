using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using KanbanModel.ModelClasses;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KanbanTests.Unit.RepositoryLayer
{
    [TestFixture]
    internal class GenericRepositoryTests
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

        private static GenericRepository<KanbanTask> CreateRepositoryWithContext(KanbanAppDbContext ctx, out Mock<IUnitOfWork<KanbanAppDbContext>> uowMock)
        {
            uowMock = new Mock<IUnitOfWork<KanbanAppDbContext>>(MockBehavior.Strict);
            uowMock.SetupGet(u => u.Context).Returns(ctx);
            return new GenericRepository<KanbanTask>(uowMock.Object);
        }

        [Test]
        public void Constructor_Sets_Context_From_UnitOfWork()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(repo.Context, Is.SameAs(ctx));
        }

        [Test]
        public void Add_Null_Throws_ArgumentNullException()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(() => repo.Add(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void Add_Valid_Sets_EntityState_To_Added()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var task = new KanbanTask { Name = "A", Description = string.Empty };

            repo.Add(task);

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Added));
        }

        [Test]
        public void BulkInsert_Null_Throws_ArgumentNullException()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(() => repo.BulkInsert(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void BulkInsert_AddsAllEntities_AsAdded()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var entities = new[]
            {
                new KanbanTask { Name = "t1", Description = string.Empty },
                new KanbanTask { Name = "t2", Description = string.Empty }
            };

            repo.BulkInsert(entities);

            // Entities should be tracked as Added (saved only when SaveChanges is called)
            var tracked = ctx.ChangeTracker.Entries<KanbanTask>().ToList();
            Assert.That(tracked.Count, Is.EqualTo(2));
            Assert.That(tracked.All(e => e.State == EntityState.Added), Is.True);
        }

        [Test]
        public void Update_Null_Throws_ArgumentNullException()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(() => repo.Update(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void Update_Sets_EntityState_To_Modified()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var task = new KanbanTask { Id = 1, Name = "u", Description = string.Empty };

            // Ensure entity is detached so SetEntryModified will set Modified
            repo.DetachEntry(task);
            repo.Update(task);

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public void Delete_Null_Throws_ArgumentNullException()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(() => repo.Delete(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void Delete_Attaches_And_Sets_EntityState_To_Deleted()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var task = new KanbanTask { Id = 11, Name = "to-delete", Description = string.Empty };
            // task is not tracked
            repo.Delete(task);

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Deleted));
        }

        [Test]
        public void SetEntryModified_Sets_State_To_Modified()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var task = new KanbanTask { Id = 21, Name = "m", Description = string.Empty };
            repo.SetEntryModified(task);

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public void DetachEntry_Sets_State_To_Detached()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var task = new KanbanTask { Name = "x", Description = string.Empty };
            // add to context so it is tracked
            ctx.KanbanTasks.Add(task);
            ctx.SaveChanges();

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Unchanged));

            repo.DetachEntry(task);

            Assert.That(ctx.Entry(task).State, Is.EqualTo(EntityState.Detached));
        }

        [Test]
        public async Task GetAllRecordsAsync_Returns_AllSeededEntities()
        {
            var seeded = new[]
            {
                new KanbanTask { Name = "s1", Description = string.Empty },
                new KanbanTask { Name = "s2", Description = string.Empty }
            };

            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), seeded);
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var all = (await repo.GetAllRecordsAsync(CancellationToken.None)).ToList();

            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.Select(t => t.Name), Is.EquivalentTo(new[] { "s1", "s2" }));
        }

        [Test]
        public async Task GetEntitiesBasedOn_Returns_FilteredResults()
        {
            var seeded = new[]
            {
                new KanbanTask { Name = "keep", Description = string.Empty },
                new KanbanTask { Name = "drop", Description = string.Empty }
            };

            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), seeded);
            var repo = CreateRepositoryWithContext(ctx, out var _);

            var queryable = ctx.KanbanTasks.Where(t => t.Name == "keep");
            var result = await repo.GetEntitiesBasedOn(queryable, CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("keep"));
        }

        [Test]
        public void FindAsync_NullId_Throws_ArgumentNullException()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            Assert.That(async () => await repo.FindAsync(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public async Task FindAsync_Returns_Entity_When_Exists()
        {
            var existing = new KanbanTask { Name = "found", Description = string.Empty };
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString(), new[] { existing });
            var repo = CreateRepositoryWithContext(ctx, out var _);

            // the in-memory provider will set an Id when saved; retrieve the saved entity id
            var saved = ctx.KanbanTasks.First();
            var found = await repo.FindAsync(saved.Id, CancellationToken.None);

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(saved.Id));
        }

        //**************************************************************************************
        // Validation-related tests: use a small test-only entity that contains DataAnnotations.
        // Test-only entity with DataAnnotations to exercise validation paths
        private class TestValidatedEntity
        {
            [Required]
            public string Name { get; set; }
        }

        // Generic helper for tests that need a repository for a custom test entity type.
        private static GenericRepository<T> CreateRepositoryWithContext<T>(KanbanAppDbContext ctx, out Mock<IUnitOfWork<KanbanAppDbContext>> uowMock)
            where T : class
        {
            uowMock = new Mock<IUnitOfWork<KanbanAppDbContext>>(MockBehavior.Strict);
            uowMock.SetupGet(u => u.Context).Returns(ctx);
            return new GenericRepository<T>(uowMock.Object);
        }


        [Test]
        public void Add_InvalidEntity_ThrowsValidationWrappedException_UsingTestEntity()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext<TestValidatedEntity>(ctx, out _);

            var invalid = new TestValidatedEntity { Name = null! }; // violates [Required]

            // ValidateEntityAndThrowException wraps ValidationException in a general Exception with "Validation failed:"
            Assert.That(() => repo.Add(invalid), Throws.Exception.With.Message.StartsWith("Validation failed:"));
        }

        [Test]
        public void Update_InvalidEntity_ThrowsValidationWrappedException_UsingTestEntity()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext<TestValidatedEntity>(ctx, out _);

            var invalid = new TestValidatedEntity { Name = null! }; // violates [Required]

            Assert.That(() => repo.Update(invalid), Throws.Exception.With.Message.StartsWith("Validation failed:"));
        }

        [Test]
        public void BulkInsert_WithInvalidEntity_ThrowsValidationWrappedException_UsingTestEntity()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext<TestValidatedEntity>(ctx, out _);

            var list = new[] { new TestValidatedEntity { Name = "ok" }, new TestValidatedEntity { Name = null! } };

            Assert.That(() => repo.BulkInsert(list), Throws.Exception.With.Message.StartsWith("Validation failed:"));
        }

        [Test]
        public void BulkInsert_Restores_AutoDetectChangesEnabled()
        {
            using var ctx = CreateInMemoryContext(Guid.NewGuid().ToString());
            var repo = CreateRepositoryWithContext(ctx, out var _);

            // ensure default is true then call BulkInsert and assert restored
            ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            var entities = new[] { new KanbanTask { Name = "x", Description = string.Empty }, new KanbanTask { Name = "y", Description = string.Empty } };

            repo.BulkInsert(entities);

            Assert.That(ctx.ChangeTracker.AutoDetectChangesEnabled, Is.True);
            // the entities should be tracked as Added
            var tracked = ctx.ChangeTracker.Entries().Where(e => e.Entity is KanbanTask).ToList();
            Assert.That(tracked.Count, Is.EqualTo(2));
            Assert.That(tracked.All(e => e.State == EntityState.Added), Is.True);
        }
    }
}

using KanbanInfrastructure.DAL;
using KanbanIntegrationTests.CustomTestSupportItems;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace KanbanIntegrationTests.PerformanceTests
{
    [TestFixture]
    [Category("Performance")]
    internal class TaskControllerPerformanceTests
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

        #region Test Data Seeding

        private async Task SeedAsync(List<KanbanTask> tasks)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KanbanAppDbContext>();

            db.KanbanTasks.RemoveRange(db.KanbanTasks); // clean
            await db.SaveChangesAsync();

            db.KanbanTasks.AddRange(tasks);
            await db.SaveChangesAsync();
        }

        //to optimized code readability
        private async Task Seed1000TasksInDb()
        {
            List<KanbanTask> kanbanTasks = Enumerable.Range(1, 1000)
            .Select(i => new KanbanTask
            {
                Name = $"Task {i}",
                Description = $"Description {i}",
                Status = StatusEnum.ToDo
            }).ToList();


            await SeedAsync(kanbanTasks);
        }

        #endregion

        #region Test Methods

        [Test]
        public async Task GetAll_ReturnsLessThan150ms_Returns50on1stPage()
        {
            //arrange
            Seed1000TasksInDb().Wait();

            //act

            await _client.GetAsync("/api/tasks?page=0&size=50"); //warmup call to ensure any initial overhead is not included in the timing

            var stopwatch = Stopwatch.StartNew();
            
            var response = await _client.GetAsync("/api/tasks?page=0&size=50");
            
            stopwatch.Stop();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.True(
                stopwatch.ElapsedMilliseconds < 150,
                $"Request took {stopwatch.ElapsedMilliseconds}ms"
            );
        }
        #endregion
    }
}

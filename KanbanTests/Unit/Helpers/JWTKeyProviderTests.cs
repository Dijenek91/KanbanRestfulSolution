using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.GraphQL.Mutations;
using KanbanRestService.Helpers;
using KanbanRestService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTests.Unit.Helpers
{
    [TestFixture]
    [Category("Unit")]
    internal class JWTKeyProviderTests
    {

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public async Task IntegrationTestEnvironment_ReturnsTestKey()
        {
            var iConfigurationMock = new Mock<IConfiguration>();          
            iConfigurationMock.Setup(config => config["TestingJwtKey"]).Returns("testing_jwt_key");

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("IntegrationTest");

            var result = JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object);

            Assert.That(result, Is.EqualTo("testing_jwt_key"));
        }

        [Test]
        public async Task UnitTestEnvironment_ReturnsTestKey()
        {
            var iConfigurationMock = new Mock<IConfiguration>();
            iConfigurationMock.Setup(config => config["TestingJwtKey"]).Returns("testing_jwt_key");

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("UnitTest");

            var result = JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object);

            Assert.That(result, Is.EqualTo("testing_jwt_key"));
        }

        [Test]
        public async Task ProductionEnvironment_ReturnsSecretKey()
        {
            var iConfigurationMock = new Mock<IConfiguration>();
            iConfigurationMock.Setup(config => config["SuperSecretJwtKey"]).Returns("testing_secret_jwt_key");
            

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("Production");

            var result = JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object);

            Assert.That(result, Is.EqualTo("testing_secret_jwt_key"));
        }

        [Test]
        public async Task ProductionEnvironment_ConfigReturnsNoKey_ExceptionThrown()
        {
            var iConfigurationMock = new Mock<IConfiguration>();
            iConfigurationMock.Setup(config => config["SuperSecretJwtKey"]).Returns((string)null);

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("Production");

            Assert.Throws<InvalidOperationException>(() => JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object));
        }

        [Test]
        public async Task IntegrationEnvironment_ConfigReturnsNoKey_ExceptionThrown()
        {
            var iConfigurationMock = new Mock<IConfiguration>();
            iConfigurationMock.Setup(config => config["TestingJwtKey"]).Returns((string)null);

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("IntegrationTest");

            Assert.Throws<InvalidOperationException>(() => JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object));
        }

        [Test]
        public async Task UnitEnvironment_ConfigReturnsNoKey_ExceptionThrown()
        {
            var iConfigurationMock = new Mock<IConfiguration>();
            iConfigurationMock.Setup(config => config["TestingJwtKey"]).Returns((string)null);

            var iHostEnvironmentMock = new Mock<IHostEnvironment>();
            iHostEnvironmentMock.Setup(environment => environment.EnvironmentName).Returns("UnitTest");

            Assert.Throws<InvalidOperationException>(() => JwtKeyProvider.GetKey(iConfigurationMock.Object, iHostEnvironmentMock.Object));
        }
    }
}

using KanbanRestService.Middlware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace KanbanTests.Unit.Middlware
{
    [TestFixture]
    [Category("Unit")]
    internal class GlobalExceptionMiddlewareTests
    {
        private static HttpContext CreateContext()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();
            return ctx;
        }

        private static void AssertResponseContains(HttpContext ctx, HttpStatusCode expectedStatus, string expectedType, string expectedMessage)
        {
            Assert.That(ctx.Response.StatusCode, Is.EqualTo((int)expectedStatus));
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            using var sr = new StreamReader(ctx.Response.Body);
            var json = sr.ReadToEnd();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.That(root.GetProperty("type").GetString(), Is.EqualTo(expectedType));
            Assert.That(root.GetProperty("error").GetString(), Does.Contain(expectedMessage));
        }

        private static void VerifyLogErrorCalled(Mock<ILogger<GlobalExceptionMiddleware>> loggerMock, Exception ex)
        {
            // Verify that Log was called with LogLevel.Error and the same exception instance.
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception occurred")),
                It.Is<Exception>(e => e == ex),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task InvokeAsync_KeyNotFoundException_Produces_404Json_And_Logs()
        {
            var ex = new KeyNotFoundException("missing");
            RequestDelegate next = _ => throw ex;
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var sut = new GlobalExceptionMiddleware(next, loggerMock.Object);

            var ctx = CreateContext();

            await sut.InvokeAsync(ctx);

            AssertResponseContains(ctx, HttpStatusCode.NotFound, nameof(KeyNotFoundException), "missing");
            VerifyLogErrorCalled(loggerMock, ex);
        }

        [Test]
        public async Task InvokeAsync_ValidationException_Produces_400Json_And_Logs()
        {
            var ex = new ValidationException("bad input");
            RequestDelegate next = _ => throw ex;
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var sut = new GlobalExceptionMiddleware(next, loggerMock.Object);

            var ctx = CreateContext();

            await sut.InvokeAsync(ctx);

            AssertResponseContains(ctx, HttpStatusCode.BadRequest, nameof(ValidationException), "bad input");
            VerifyLogErrorCalled(loggerMock, ex);
        }

        [Test]
        public async Task InvokeAsync_UnauthorizedAccessException_Produces_401Json_And_Logs()
        {
            var ex = new UnauthorizedAccessException("no auth");
            RequestDelegate next = _ => throw ex;
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var sut = new GlobalExceptionMiddleware(next, loggerMock.Object);

            var ctx = CreateContext();

            await sut.InvokeAsync(ctx);

            AssertResponseContains(ctx, HttpStatusCode.Unauthorized, nameof(UnauthorizedAccessException), "no auth");
            VerifyLogErrorCalled(loggerMock, ex);
        }

        [Test]
        public async Task InvokeAsync_GenericException_Produces_500Json_And_Logs()
        {
            var ex = new Exception("boom");
            RequestDelegate next = _ => throw ex;
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var sut = new GlobalExceptionMiddleware(next, loggerMock.Object);

            var ctx = CreateContext();

            await sut.InvokeAsync(ctx);

            AssertResponseContains(ctx, HttpStatusCode.InternalServerError, nameof(Exception), "boom");
            VerifyLogErrorCalled(loggerMock, ex);
        }
    }
}
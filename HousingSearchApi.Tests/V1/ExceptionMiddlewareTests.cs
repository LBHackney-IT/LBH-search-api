using FluentAssertions;
using HousingSearchApi.V1;
using HousingSearchApi.V1.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSearchApi.Tests.V1
{
    [TestFixture]
    public class ExceptionMiddlewareTests
    {
        private readonly string _traceId = Guid.NewGuid().ToString();
        private readonly string _correlationId = Guid.NewGuid().ToString();
        private const int DEFAULTERRORCODE = 500;
        private const string DEFAULTERRORMESSAGE = "Internal Server Error.";

        private HttpContext _httpContext;
        private Mock<IExceptionHandlerFeature> _mockExHandlerFeature;
        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _httpContext = new DefaultHttpContext();
            _httpContext.TraceIdentifier = _traceId;
            _httpContext.Request.Headers.Add(Constants.CorrelationId, new StringValues(_correlationId));
            _httpContext.Response.Body = new MemoryStream();
            _httpContext.Response.StatusCode = DEFAULTERRORCODE;

            _mockExHandlerFeature = new Mock<IExceptionHandlerFeature>();
            _httpContext.Features.Set(_mockExHandlerFeature.Object);
            _mockLogger = new Mock<ILogger>();
        }

        private async Task VerifyResponse(string resultMessage = DEFAULTERRORMESSAGE, int statusCode = DEFAULTERRORCODE)
        {
            _httpContext.Response.ContentType.Should().Be("application/json");

            var expected = new ExceptionResult(resultMessage, _traceId, _correlationId, statusCode).ToString();
            _httpContext.Response.Body.Position = 0;
            using (StreamReader streamReader = new StreamReader(_httpContext.Response.Body))
            {
                string actual = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                expected.Should().Be(actual);
            }
        }

        [Test]
        public async Task HandleExceptionsTestNoExceptionHandlerWritesResponse()
        {
            // Arrange
            _httpContext.Features.Set<IExceptionHandlerFeature>(null);

            // Act
            await ExceptionMiddlewareExtensions.HandleExceptions(_httpContext, _mockLogger.Object)
                                               .ConfigureAwait(false);

            // Assert
            await VerifyResponse().ConfigureAwait(false);
        }

        [Test]
        public async Task HandleExceptionsTestWithHandlerButNoExceptionWritesResponse()
        {
            // Act
            await ExceptionMiddlewareExtensions.HandleExceptions(_httpContext, _mockLogger.Object)
                                               .ConfigureAwait(false);

            // Assert
            await VerifyResponse().ConfigureAwait(false);

            _mockLogger.VerifyExact(LogLevel.Error, "Request failed.", Times.Once());
        }

        [Test]
        public async Task HandleExceptionsTestWithHandlerWithExceptionWritesResponse()
        {
            // Arrange
            var exMessage = "This is an exception";
            var exception = new Exception(exMessage);
            _mockExHandlerFeature.SetupGet(x => x.Error).Returns(exception);

            // Act
            await ExceptionMiddlewareExtensions.HandleExceptions(_httpContext, _mockLogger.Object)
                                               .ConfigureAwait(false);

            // Assert
            await VerifyResponse(exMessage).ConfigureAwait(false);
            _mockLogger.VerifyContains(LogLevel.Error, "Request failed.", Times.Once());
        }
    }
}
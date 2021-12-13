using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ScrapperWorker.Clients;
using ScrapperWorker.Models;
using ScrapperWorker.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapperWorker.Tests.Clients
{
    public class TvMazeApiClientTests
    {
        private Fixture _fixture;
        private Mock<ILogger<TvShowScrapperService>> _loggerMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _loggerMock = _mockRepository.Create<ILogger<TvShowScrapperService>>();
            _loggerMock.Setup(x => x.Log(
                                  It.IsAny<LogLevel>(),
                                  It.IsAny<EventId>(),
                                  It.IsAny<It.IsAnyType>(),
                                  It.IsAny<Exception>(),
                                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            _httpMessageHandlerMock = _mockRepository.Create<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClient.BaseAddress = new Uri("https://tvapimock.com");
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        #region LoadShowsFromTvMazeApiByPageNumber

        [Test]
        public async Task LoadShowsFromTvMazeApiByPageNumber_ShouldReturnValidListOfShows()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedShows = _fixture.CreateMany<Show>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expectedShows))
                });

            // Act
            var shows = await tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(_fixture.Create<int>(), default);

            // Assert
            shows.Should().NotBeNull();
            shows.Should().BeEquivalentTo(expectedShows);
        }

        [Test]
        public async Task LoadShowsFromTvMazeApiByPageNumber_ShouldReturnEmptyList_WhenThereAreNoShows()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedShows = new List<Show>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expectedShows))
                });

            // Act
            var shows = await tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(_fixture.Create<int>(), default);

            // Assert
            shows.Should().NotBeNull();
            shows.Should().BeEmpty();
        }

        [Test]
        public async Task LoadShowsFromTvMazeApiByPageNumber_ShouldReturnNull_WhenPageIsNotFound()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("NotFound", new Exception("NotFound"), HttpStatusCode.NotFound));

            // Act
            var shows = await tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(_fixture.Create<int>(), default);

            // Assert
            shows.Should().BeNull();
        }

        [Test]
        public async Task LoadShowsFromTvMazeApiByPageNumber_ShouldThrow_WhenHttpRequestException_OtherThanNotFound()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedShows = new List<Show>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("BadRequest", new Exception("BadRequest"), HttpStatusCode.BadRequest));

            // Act
            Func<Task> showFromApiAction = async () => await tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(_fixture.Create<int>(), default);

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task LoadShowsFromTvMazeApiByPageNumber_ShouldThrow_WhenException()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedShows = new List<Show>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("SomeError"));

            // Act
            Func<Task> showFromApiAction = async () => await tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(_fixture.Create<int>(), default);

            // Assert
            await showFromApiAction.Should().ThrowAsync<Exception>().WithMessage("SomeError");
        }

        #endregion LoadShowsFromTvMazeApiByPageNumber

        #region LoadCastFromTvMazeApi

        [Test]
        public async Task LoadCastFromTvMazeApi_ShouldReturnValidListOfCast()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedCastList = _fixture.CreateMany<Cast>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expectedCastList))
                });

            // Act
            var castList = await tvMazeApiClient.LoadCastFromTvMazeApi(_fixture.Create<int>(), default);

            // Assert
            castList.Should().NotBeNull();
            castList.Should().BeEquivalentTo(expectedCastList);
        }

        [Test]
        public async Task LoadCastFromTvMazeApi_ShouldReturnEmptyList_WhenThereAreNoCast()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedCastList = new List<Cast>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expectedCastList))
                });

            // Act
            var castList = await tvMazeApiClient.LoadCastFromTvMazeApi(_fixture.Create<int>(), default);

            // Assert
            castList.Should().NotBeNull();
            castList.Should().BeEmpty();
        }

        [Test]
        public async Task LoadCastFromTvMazeApi_ShouldThrow_WhenHttpRequestException()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedCastList = new List<Cast>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("BadRequest", new Exception("BadRequest"), HttpStatusCode.BadRequest));

            // Act
            Func<Task> castListFromApiAction = async () => await tvMazeApiClient.LoadCastFromTvMazeApi(_fixture.Create<int>(), default);

            // Assert
            await castListFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task LoadCastFromTvMazeApi_ShouldThrow_WhenException()
        {
            // Arrange
            var tvMazeApiClient = new TvMazeApiClient(_loggerMock.Object, _httpClient);

            var expectedCastList = new List<Cast>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("SomeError"));

            // Act
            Func<Task> castListFromApiAction = async () => await tvMazeApiClient.LoadCastFromTvMazeApi(_fixture.Create<int>(), default);

            // Assert
            await castListFromApiAction.Should().ThrowAsync<Exception>().WithMessage("SomeError");
        }

        #endregion LoadCastFromTvMazeApi
    }
}
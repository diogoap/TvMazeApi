using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Repository;
using ScrapperWorker.Clients;
using ScrapperWorker.Models;
using ScrapperWorker.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrapperWorker.Tests.Services
{
    public class TvShowScrapperServiceTests
    {
        private Fixture _fixture;
        private Mock<ILogger<TvShowScrapperService>> _loggerMock;
        private Mock<ITvMazeApiClient> _tvMazeApiClientMock;
        private Mock<IShowRepository> _showRepositoryMock;
        private Mock<IShowPageRepository> _showPageRepositoryMock;
        private MockRepository? _mockRepository;

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

            _tvMazeApiClientMock = _mockRepository.Create<ITvMazeApiClient>();
            _showRepositoryMock = _mockRepository.Create<IShowRepository>();
            _showPageRepositoryMock = _mockRepository.Create<IShowPageRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        #region LoadShows

        [Test]
        public async Task LoadShows_ShouldScrapUntilNoShowsReturned()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);

            var lastStoredPage = -1; // Last page will be 0
            int GetLastStoredPage()
            {
                lastStoredPage++;
                return lastStoredPage;
            }

            _showPageRepositoryMock.Setup(x => x.GetLastStoredPage()).Returns(GetLastStoredPage);

            var showsFromApiPage1 = CreateShowsAndSetupMocks(10, 1);
            var showsFromApiPage2 = CreateShowsAndSetupMocks(10, 2);
            var showsFromApiPage3 = CreateShowsAndSetupMocks(5, 3);
            var showsFromApiPage4 = CreateShowsAndSetupMocks(0, 4);

            // Act
            await tvShowScrapperService.LoadShows(default);

            // Assert
            _showPageRepositoryMock.Verify(x => x.AddShowPage(0), Times.Never);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(1), Times.Once);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(2), Times.Once);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(3), Times.Once);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(4), Times.Never);
        }

        [Test]
        public async Task LoadShows_ShouldRestartScrapFromLastPage()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);

            var lastStoredPage = 2; //Last page will be 3
            int GetLastStoredPage()
            {
                lastStoredPage++;
                return lastStoredPage;
            }

            _showPageRepositoryMock.Setup(x => x.GetLastStoredPage()).Returns(GetLastStoredPage);

            var showsFromApiPage4 = CreateShowsAndSetupMocks(10, 4);
            var showsFromApiPage5 = CreateShowsAndSetupMocks(10, 5);
            var showsFromApiPage6 = CreateShowsAndSetupMocks(0, 6);

            // Act
            await tvShowScrapperService.LoadShows(default);

            // Assert
            _showPageRepositoryMock.Verify(x => x.AddShowPage(0), Times.Never);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(1), Times.Never);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(2), Times.Never);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(3), Times.Never);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(4), Times.Once);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(5), Times.Once);
            _showPageRepositoryMock.Verify(x => x.AddShowPage(6), Times.Never);
        }

        [Test]
        public async Task LoadShows_ShouldExitWhenThereAreNoShows()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);

            var lastStoredPage = -1; //Last page will be 0
            int GetLastStoredPage()
            {
                lastStoredPage++;
                return lastStoredPage;
            }

            _showPageRepositoryMock.Setup(x => x.GetLastStoredPage()).Returns(GetLastStoredPage);

            var showsFromApiPage1 = CreateShowsAndSetupMocks(0, 1);

            // Act
            await tvShowScrapperService.LoadShows(default);

            // Assert
            _showPageRepositoryMock.Verify(x => x.AddShowPage(It.IsAny<int>()), Times.Never);
        }

        #endregion LoadShows

        #region LoadShowsByPageNumber

        [Test]
        public async Task LoadShowsByPageNumber_ShouldReturnTrue_WhenShowsFound()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);
            var pageNumber = _fixture.Create<int>();

            var showFromApi = _fixture.Build<Show>().Without(x => x.Cast).Create();
            _tvMazeApiClientMock.Setup(x => x.LoadShowsFromTvMazeApiByPageNumber(pageNumber, default)).ReturnsAsync(new List<Show> { showFromApi });
            _showRepositoryMock.Setup(x => x.GetShow(showFromApi.Id)).Returns(null as Domain.Models.Show);

            var castList = _fixture.CreateMany<Cast>();
            _tvMazeApiClientMock.Setup(x => x.LoadCastFromTvMazeApi(showFromApi.Id, default)).ReturnsAsync(castList);

            Domain.Models.Show? showSaved = null;
            _showRepositoryMock.Setup(x => x.AddShow(It.IsAny<Domain.Models.Show>())).
                Callback((Domain.Models.Show showToSave) => showSaved = showToSave);

            _showPageRepositoryMock.Setup(x => x.AddShowPage(pageNumber));

            // Act
            var result = await tvShowScrapperService.LoadShowsByPageNumber(pageNumber, default);

            // Assert
            result.Should().BeTrue();
            showSaved.Should().BeEquivalentTo(Show.MapToDbModel(showFromApi, castList));
        }

        [Test]
        public async Task LoadShowsByPageNumber_ShouldReturnFalse_WhenNoShowsFound()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);
            var pageNumber = _fixture.Create<int>();
            _tvMazeApiClientMock.Setup(x => x.LoadShowsFromTvMazeApiByPageNumber(pageNumber, default)).ReturnsAsync(null as IEnumerable<Show>);

            // Act
            var result = await tvShowScrapperService.LoadShowsByPageNumber(pageNumber, default);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task LoadShowsByPageNumber_ShouldReturnTrue_ButNotSaveShow_WhenShowAlreadyExists()
        {
            // Arrange
            var tvShowScrapperService = new TvShowScrapperService(_loggerMock.Object, _tvMazeApiClientMock.Object, _showRepositoryMock.Object, _showPageRepositoryMock.Object);
            var pageNumber = _fixture.Create<int>();

            var showFromApi = _fixture.Build<Show>().Without(x => x.Cast).Create();
            _tvMazeApiClientMock.Setup(x => x.LoadShowsFromTvMazeApiByPageNumber(pageNumber, default)).ReturnsAsync(new List<Show> { showFromApi });

            var showDomainModel = new Domain.Models.Show
            {
                Id = showFromApi.Id,
                Name = showFromApi.Name
            };
            _showRepositoryMock.Setup(x => x.GetShow(showFromApi.Id)).Returns(showDomainModel);
            _showPageRepositoryMock.Setup(x => x.AddShowPage(pageNumber));

            // Act
            var result = await tvShowScrapperService.LoadShowsByPageNumber(pageNumber, default);

            // Assert
            result.Should().BeTrue();
        }

        #endregion LoadShowsByPageNumber

        private IEnumerable<Show>? CreateShowsAndSetupMocks(int numberOfShows, int pageNumber)
        {
            if (numberOfShows == 0)
            {
                _tvMazeApiClientMock.Setup(x => x.LoadShowsFromTvMazeApiByPageNumber(pageNumber, default)).ReturnsAsync(null as IEnumerable<Show>);
                return null;
            }

            var showsPage = _fixture.Build<Show>().Without(x => x.Cast).CreateMany(numberOfShows);
            _tvMazeApiClientMock.Setup(x => x.LoadShowsFromTvMazeApiByPageNumber(pageNumber, default)).ReturnsAsync(showsPage);

            foreach (var show in showsPage)
            {
                _showRepositoryMock.Setup(x => x.GetShow(show.Id)).Returns(null as Domain.Models.Show);

                var castList = _fixture.CreateMany<Cast>();
                _tvMazeApiClientMock.Setup(x => x.LoadCastFromTvMazeApi(show.Id, default)).ReturnsAsync(castList);

                _showRepositoryMock.Setup(x => x.AddShow(It.IsAny<Domain.Models.Show>()));
            }

            _showPageRepositoryMock.Setup(x => x.AddShowPage(pageNumber));

            return showsPage;
        }
    }
}
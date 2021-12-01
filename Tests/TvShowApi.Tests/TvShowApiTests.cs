using AutoFixture;
using Domain.Models;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TvShowApi.Tests
{
    public class TvShowApiTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Customizations.Add(new DateOfBirthDateTimeBuilder());
        }

        [Test]
        public async Task Api_ShouldReturnNotFound_WhenResourceDoesNotExist()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            // Act
            Func<Task> showFromApiAction = async () => await client.GetFromJsonAsync<Show>($"/MYshows");

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetShowById_ShouldReturnValidShow_WhenShowExists()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            var show = _fixture.Create<Show>();
            var response = await client.PostAsJsonAsync("/shows", show);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act
            var showFromApi = await client.GetFromJsonAsync<Show>($"/shows/{show.Id}");

            // Assert
            showFromApi.Should().NotBeNull();
            showFromApi.Should().BeEquivalentTo(show);
        }

        [Test]
        public async Task GetShowById_ShouldReturnNotFound_WhenShowDoesNotExist()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();
            var showId = _fixture.Create<int>();

            // Act
            Func<Task> showFromApiAction = async () => await client.GetFromJsonAsync<Show>($"/shows/{showId}");

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetShows_ShouldReturnValidNumberOfPages()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();
            var shows = new List<Show>();

            for (int i = 1; i <= 5; i++)
            {
                var show = _fixture.Build<Show>().With(s => s.Id, i).Create();
                shows.Add(show);
                var response = await client.PostAsJsonAsync("/shows", show);
                response.Should().NotBeNull();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            // Act
            var showsFromApiPage1 = await client.GetFromJsonAsync<IList<Show>>($"/shows?pageNumber={1}&pageSize={2}");
            var showsFromApiPage2 = await client.GetFromJsonAsync<IList<Show>>($"/shows?pageNumber={2}&pageSize={2}");
            var showsFromApiPage3 = await client.GetFromJsonAsync<IList<Show>>($"/shows?pageNumber={3}&pageSize={2}");

            // Assert
            showsFromApiPage1.Should().NotBeNull();
            showsFromApiPage1?.Count.Should().Be(2);
            showsFromApiPage1?[0].Should().BeEquivalentTo(shows[0]);
            showsFromApiPage1?[1].Should().BeEquivalentTo(shows[1]);

            showsFromApiPage2.Should().NotBeNull();
            showsFromApiPage2?.Count.Should().Be(2);
            showsFromApiPage2?[0].Should().BeEquivalentTo(shows[2]);
            showsFromApiPage2?[1].Should().BeEquivalentTo(shows[3]);

            showsFromApiPage3.Should().NotBeNull();
            showsFromApiPage3?.Count.Should().Be(1);
            showsFromApiPage3?[0].Should().BeEquivalentTo(shows[4]);
        }

        [Test]
        public async Task GetShows_ShouldReturnCastOrderedByBirthday_InDescendingOrder()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            for (int i = 1; i <= 10; i++)
            {
                var show = _fixture.Build<Show>().With(s => s.Id, i).Create();
                var response = await client.PostAsJsonAsync("/shows", show);
                response.Should().NotBeNull();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            // Act
            var showsFromApi = await client.GetFromJsonAsync<IList<Show>>($"/shows?pageNumber={1}&pageSize={10}");

            // Assert
            showsFromApi.Should().NotBeNull();
            showsFromApi?.Count.Should().Be(10);
            foreach (var show in showsFromApi)
            {
                show.Cast.Should().BeInDescendingOrder(c => c.Birthday);
            }
        }

        [Test]
        public async Task GetShows_ShouldReturnAllItems_IfPageSizeIsHigherThanItemsInDatabase()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            for (int i = 1; i <= 5; i++)
            {
                var show = _fixture.Build<Show>().With(s => s.Id, i).Create();
                var response = await client.PostAsJsonAsync("/shows", show);
                response.Should().NotBeNull();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            // Act
            var showsFromApi = await client.GetFromJsonAsync<IList<Show>>($"/shows?pageNumber={1}&pageSize={100}");

            // Assert
            showsFromApi.Should().NotBeNull();
            showsFromApi?.Count.Should().Be(5);
        }

        [Test]
        public async Task GetShows_ShouldReturnNotFound_IfThereAreNoItemsInDatabase()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            // Act
            Func<Task> showFromApiAction = async () => await client.GetFromJsonAsync<Show>($"/shows?pageNumber={1}&pageSize={10}");

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetShows_ShouldReturnNotFound_IfPageRequestedDoesNotExist()
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            for (int i = 1; i <= 5; i++)
            {
                var show = _fixture.Build<Show>().With(s => s.Id, i).Create();
                var response = await client.PostAsJsonAsync("/shows", show);
                response.Should().NotBeNull();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            // Act
            Func<Task> showFromApiAction = async () => await client.GetFromJsonAsync<Show>($"/shows?pageNumber={2}&pageSize={5}");

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCase("?pageNumber=&pageSize=5")]
        [TestCase("?pageNumber=1&pageSize=")]
        [TestCase("?pageNumber1&pageSize=5")]
        [TestCase("?pageNumber=1&pageSize5")]
        [TestCase("?MYpageNumber=1&pageSize=5")]
        [TestCase("?pageNumber=1&MYpageSize=5")]
        [TestCase("?pageNumber=1.0&MYpageSize=5")]
        [TestCase("?pageNumber=1&MYpageSize=5.0")]
        [TestCase("?pageNumber=1?pageSize=5")]
        [TestCase("?pageNumber=1")]
        [TestCase("?pageSize=5")]

        public async Task GetShows_ShouldReturnBadRequest_IfQueryStringIsInvalid(string queryString)
        {
            // Arrange
            await using var application = new TvShowTestApplication();
            var client = application.CreateClient();

            var show = _fixture.Create<Show>();
            var response = await client.PostAsJsonAsync("/shows", show);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act
            Func<Task> showFromApiAction = async () => await client.GetFromJsonAsync<Show>($"/shows{queryString}");

            // Assert
            await showFromApiAction.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
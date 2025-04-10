// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kumara.Tests.EventSource
{
    [TestClass]
    public class ContentTypeTests
    {
        private readonly HttpClient _httpClient;
        private readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();

        public ContentTypeTests()
        {
            WebApplicationFactory<Program> factory =
                new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                    builder.ConfigureServices(services => services.AddSingleton(_eventRepository))
                );

            _httpClient = factory.CreateClient();
        }

        [TestMethod]
        public async Task GetEvents_ReturnsCorrectContentType()
        {
            // Arrange
            var expectedContentType = "application/json";
            var expectedEvent = new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Expected Test String",
                        TestEnum = TestOptions.OptionB,
                        TestInteger = 100,
                    }
                ),
            };

            await _eventRepository.AddEventsAsync(new[] { expectedEvent });

            // Act
            var response = await _httpClient.GetAsync("/events");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var contentType = response.Content.Headers.ContentType?.MediaType;
            contentType.ShouldBe(expectedContentType);

            var events = await response.Content.ReadFromJsonAsync<List<EventEntity>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // Assert
            events.ShouldNotBeNull();
            events.Count.ShouldBe(1);
            events[0]
                .ShouldSatisfyAllConditions(
                    e => e.ITwinGuid.ShouldBe(expectedEvent.ITwinGuid),
                    e => e.AccountGuid.ShouldBe(expectedEvent.AccountGuid),
                    e => e.CorrelationId.ShouldBe(expectedEvent.CorrelationId),
                    e => e.Id.ShouldBe(expectedEvent.Id),
                    e => e.SpecVersion.ShouldBe(expectedEvent.SpecVersion),
                    e => e.Source.ShouldBe(expectedEvent.Source),
                    e => e.Type.ShouldBe(expectedEvent.Type),
                    e =>
                        JsonSerializer
                            .Serialize(e.DataJson)
                            .ShouldBe(JsonSerializer.Serialize(expectedEvent.DataJson))
                );
        }

        [TestMethod]
        public async Task PostEvents_AcceptsCorrectContentType()
        {
            // Arrange
            var eventsPayload = new List<EventEntity>
            {
                new EventEntity
                {
                    ITwinGuid = Guid.NewGuid(),
                    AccountGuid = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri("http://example.com/TestSource"),
                    Type = "test.created.v1",
                    DataJson = JsonSerializer.SerializeToDocument(
                        new TestCreatedV1
                        {
                            TestString = "Payload Test String",
                            TestEnum = TestOptions.OptionC,
                            TestInteger = 200,
                        }
                    ),
                },
            };

            var serialized = JsonSerializer.Serialize(eventsPayload);
            StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/events", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task PostEvents_RejectsIncorrectContentType()
        {
            // Arrange
            StringContent content = new("Invalid content", System.Text.Encoding.UTF8, "text/plain");

            // Act
            var response = await _httpClient.PostAsync("/events", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
        }
    }
}

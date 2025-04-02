// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Kumara.EventSource.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Kumara.Tests.EventSource
{
    [TestClass]
    public sealed class ContentTests
    {
        private readonly HttpClient _client;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly String _endpoint = "/events";

        public ContentTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();

            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockEventRepository.Object);
                });
            });

            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task PostEvents_WithZeroEvents_ReturnsSuccessAndCountZero()
        {
            // Act
            var content = new StringContent(
                "[]",
                System.Text.Encoding.UTF8,
                "application/cloudevents-batch+json"
            );
            var response = await _client.PostAsync(_endpoint, content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNull();
            responseString.ShouldContain("\"count\":0");

            _mockEventRepository.Verify(
                repo => repo.AddEventsAsync(It.IsAny<IEnumerable<CloudEvent>>()),
                Times.Never
            );
        }

        [TestMethod]
        public async Task PostEvents_WithMultipleEvents_ReturnsSuccessAndCorrectCount()
        {
            var eventsPayload = new List<CloudEvent>
            {
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "test.created.v1",
                    Source = new Uri("/events/test"),
                    Id = "T234-1234-1234",
                    Time = DateTimeOffset.Parse("2023-10-01T12:15:00Z"),
                    Data = new
                    {
                        test_string = "Sample Test 1",
                        test_enum = "OptionA",
                        test_integer = 123,
                        event_type_version = "1.0",
                    },
                },
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "test.created.v1",
                    Source = new Uri("/events/test"),
                    Id = "T234-1234-1235",
                    Time = DateTimeOffset.Parse("2023-10-01T12:20:00Z"),
                    Data = new
                    {
                        test_string = "Sample Test 2",
                        test_enum = "OptionB",
                        test_integer = 456,
                        event_type_version = "1.0",
                    },
                },
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "test.created.v1",
                    Source = new Uri("/events/test"),
                    Id = "T234-1234-1236",
                    Time = DateTimeOffset.Parse("2023-10-01T12:25:00Z"),
                    Data = new
                    {
                        test_string = "Sample Test 3",
                        test_enum = "OptionC",
                        test_integer = 789,
                        event_type_version = "1.0",
                    },
                },
            };

            // Act
            var formatter = new JsonEventFormatter();
            var encodedContent = formatter.EncodeBatchModeMessage(
                eventsPayload,
                out var contentType
            );

            var content = new ByteArrayContent(encodedContent.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                contentType.MediaType
            );
            var response = await _client.PostAsync(_endpoint, content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNull();
            responseString.ShouldContain("\"count\":3");

            _mockEventRepository.Verify(
                repo => repo.AddEventsAsync(It.IsAny<IEnumerable<CloudEvent>>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task GetEvents_ReturnsCloudEventBatch()
        {
            // Arrange
            var cloudEvents = new List<CloudEvent>
            {
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "test.created.v1",
                    Source = new Uri("/source/user"),
                    Id = "A234-1234-1234",
                    Time = DateTimeOffset.UtcNow,
                },
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "test.created.v1",
                    Source = new Uri("/source/file"),
                    Id = "B234-1234-1234",
                    Time = DateTimeOffset.UtcNow,
                },
            }.AsQueryable();

            _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(cloudEvents);

            // Act
            var response = await _client.GetAsync("/events");

            // Assert
            response.EnsureSuccessStatusCode();
            response
                .Content.Headers.ContentType?.ToString()
                .ShouldBe("application/cloudevents-batch+json; charset=utf-8");

            var formatter = new JsonEventFormatter();
            var returnedCloudEvents = await response.ToCloudEventBatchAsync(formatter);

            returnedCloudEvents.ShouldNotBeNull();
            returnedCloudEvents.Count().ShouldBe(2);

            _mockEventRepository.Verify(repo => repo.GetAllEventsAsync(), Times.Once);
        }
    }
}

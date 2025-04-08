// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Mime;
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

namespace Kumara.Tests.EventSource;

[TestClass]
public sealed class ContentTests
{
    private readonly HttpClient _client;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly string _endpoint = "/events";

    public ContentTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();

        WebApplicationFactory<Program> factory =
            new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
        StringContent content = new(
            "[]",
            System.Text.Encoding.UTF8,
            "application/cloudevents-batch+json"
        );
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
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
        List<CloudEvent> eventsPayload = new()
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
        JsonEventFormatter formatter = new();
        ReadOnlyMemory<byte> encodedContent = formatter.EncodeBatchModeMessage(
            eventsPayload,
            out ContentType contentType
        );

        ByteArrayContent content = new(encodedContent.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            contentType.MediaType
        );
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
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
        IQueryable<CloudEvent> cloudEvents = new List<CloudEvent>
        {
            new(CloudEventsSpecVersion.V1_0)
            {
                Type = "UserLogin",
                Source = new Uri("/source/user"),
                Id = "A234-1234-1234",
                Time = DateTimeOffset.UtcNow,
            },
            new(CloudEventsSpecVersion.V1_0)
            {
                Type = "FileUpload",
                Source = new Uri("/source/file"),
                Id = "B234-1234-1234",
                Time = DateTimeOffset.UtcNow,
            },
        }.AsQueryable();

        _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(cloudEvents);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/events");

        // Assert
        response.EnsureSuccessStatusCode();
        response
            .Content.Headers.ContentType?.ToString()
            .ShouldBe("application/cloudevents-batch+json; charset=utf-8");

        JsonEventFormatter formatter = new();
        IReadOnlyList<CloudEvent> returnedCloudEvents = await response.ToCloudEventBatchAsync(
            formatter
        );

        returnedCloudEvents.ShouldNotBeNull();
        returnedCloudEvents.Count().ShouldBe(2);

        _mockEventRepository.Verify(repo => repo.GetAllEventsAsync(), Times.Once);
    }
}

// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

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
public sealed class ContentTypeTests
{
    private readonly HttpClient _client;
    private readonly string _endpoint = "/events";
    private readonly Mock<IEventRepository> _mockEventRepository;

    public ContentTypeTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();

        WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
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
    public async Task PostSingleEvent_WithSupportedContentType_ReturnsSuccess()
    {
        CloudEvent cloudEvent = new(CloudEventsSpecVersion.V1_0)
        {
            Type = "UserLogin",
            Source = new Uri("/source/user"),
            Id = "A234-1234-1234",
            Time = DateTimeOffset.UtcNow
        };

        JsonEventFormatter formatter = new();
        ReadOnlyMemory<byte> encodedContent =
            formatter.EncodeStructuredModeMessage(cloudEvent, out ContentType contentType);

        ByteArrayContent content = new(encodedContent.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType.MediaType);
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":1");

        _mockEventRepository.Verify(repo => repo.AddEventsAsync(It.IsAny<IEnumerable<CloudEvent>>()), Times.Once);
    }

    [TestMethod]
    public async Task PostBatchEvents_WithSupportedContentType_ReturnsSuccess()
    {
        List<CloudEvent> eventsPayload = new()
        {
            new CloudEvent(CloudEventsSpecVersion.V1_0)
            {
                Type = "UserLogin",
                Source = new Uri("/source/user"),
                Id = "A234-1234-1234",
                Time = DateTimeOffset.UtcNow
            },
            new CloudEvent(CloudEventsSpecVersion.V1_0)
            {
                Type = "FileUpload",
                Source = new Uri("/source/file"),
                Id = "B234-1234-1234",
                Time = DateTimeOffset.UtcNow
            }
        };

        JsonEventFormatter formatter = new();
        ReadOnlyMemory<byte> encodedContent =
            formatter.EncodeBatchModeMessage(eventsPayload, out ContentType contentType);

        ByteArrayContent content = new(encodedContent.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType.MediaType);
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":2");

        _mockEventRepository.Verify(repo => repo.AddEventsAsync(It.IsAny<IEnumerable<CloudEvent>>()), Times.Once);
    }

    [TestMethod]
    public async Task PostUnsupportedMediaType_ReturnsProblemDetails()
    {
        StringContent content = new("{}", Encoding.UTF8, "application/xml");
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
        response.Content.Headers.ContentType?.ToString().ShouldBe("application/problem+json");

        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("Unsupported Media Type");
        responseString.ShouldContain("The provided content type is not supported.");
    }

    [TestMethod]
    public async Task GetEvents_ReturnsCorrectContentType()
    {
        IQueryable<CloudEvent> cloudEvents = new List<CloudEvent>
        {
            new(CloudEventsSpecVersion.V1_0)
            {
                Type = "UserLogin",
                Source = new Uri("/source/user"),
                Id = "A234-1234-1234",
                Time = DateTimeOffset.UtcNow
            }
        }.AsQueryable();

        _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(cloudEvents);

        HttpResponseMessage response = await _client.GetAsync(_endpoint);

        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().ShouldBe("application/cloudevents-batch+json; charset=utf-8");

        JsonEventFormatter formatter = new();
        IReadOnlyList<CloudEvent> returnedCloudEvents = await response.ToCloudEventBatchAsync(formatter);

        returnedCloudEvents.ShouldNotBeNull();
        returnedCloudEvents.Count().ShouldBe(1);

        _mockEventRepository.Verify(repo => repo.GetAllEventsAsync(), Times.Once);
    }
}

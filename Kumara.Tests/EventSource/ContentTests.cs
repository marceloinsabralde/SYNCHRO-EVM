// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Mime;
using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
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
        StringContent content = new("[]", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":0");

        _mockEventRepository.Verify(
            repo => repo.AddEventsAsync(It.IsAny<IEnumerable<EventEntity>>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task PostEvents_WithMultipleEvents_ReturnsSuccessAndCorrectCount()
    {
        List<EventEntity> eventsPayload = new()
        {
            new EventEntity
            {
                Type = "test.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Sample Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 42,
                    }
                ),
            },
            new EventEntity
            {
                Type = "test.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Sample Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 42,
                    }
                ),
            },
            new EventEntity
            {
                Type = "test.created.v1",
                Source = new Uri("/events/test"),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Sample Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 42,
                    }
                ),
            },
        };

        // Act
        var serialized = JsonSerializer.Serialize(eventsPayload);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync(_endpoint, content);

        // Assert
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.ShouldNotBeNull();
        responseString.ShouldContain("\"count\":3");

        _mockEventRepository.Verify(
            repo => repo.AddEventsAsync(It.IsAny<IEnumerable<EventEntity>>()),
            Times.Once
        );
    }

    [TestMethod]
    public async Task GetEvents_ReturnsEventEntityBatch()
    {
        // Arrange
        IQueryable<EventEntity> eventEntities = new List<EventEntity>
        {
            new()
            {
                Type = "UserLogin",
                Source = new Uri("/source/user"),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            new()
            {
                Type = "FileUpload",
                Source = new Uri("/source/file"),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
        }.AsQueryable();

        _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(eventEntities);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/events");

        // Assert
        response.EnsureSuccessStatusCode();
        response
            .Content.Headers.ContentType?.ToString()
            .ShouldBe("application/json; charset=utf-8");

        var responseString = await response.Content.ReadAsStringAsync();
        var returnedEventEntities = JsonSerializer.Deserialize<List<EventEntity>>(
            responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        returnedEventEntities.ShouldNotBeNull();
        returnedEventEntities.Count().ShouldBe(2);

        _mockEventRepository.Verify(repo => repo.GetAllEventsAsync(), Times.Once);
    }

    [TestMethod]
    public void ValidateEventEntitySerialization()
    {
        // Arrange
        var eventEntity = new EventEntity
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "TestType",
        };

        // Act
        var serialized = JsonSerializer.Serialize(eventEntity);
        var deserialized = JsonSerializer.Deserialize<EventEntity>(serialized);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ITwinGuid.ShouldBe(eventEntity.ITwinGuid);
        deserialized.AccountGuid.ShouldBe(eventEntity.AccountGuid);
    }
}

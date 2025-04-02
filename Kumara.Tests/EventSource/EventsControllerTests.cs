// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Kumara.EventSource.Controllers;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class EventsControllerTests
{
    private readonly HttpClient _client;

    private readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();

    public EventsControllerTests()
    {
        WebApplicationFactory<EventsController> factory =
            new WebApplicationFactory<EventsController>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddSingleton(_eventRepository))
            );

        _client = factory.CreateClient();
    }

    [TestMethod]
    public async Task PostEvents_ValidEventEntity_ReturnsOk()
    {
        EventEntity eventEntity = new()
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
                    TestString = "Controller Test String",
                    TestEnum = TestOptions.OptionE,
                    TestInteger = 500,
                }
            ),
        };

        string serialized = JsonSerializer.Serialize(new[] { eventEntity });
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_InvalidMediaType_ReturnsUnsupportedMediaType()
    {
        StringContent content = new("Invalid content");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "text/plain"
        );

        // Submit POST request with invalid media type
        HttpResponseMessage response = await _client.PostAsync("/events", content);

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
    }

    [TestMethod]
    public async Task RoundTripEvents_ValidEvents_ReturnsEventEntities()
    {
        List<EventEntity> eventEntities = new()
        {
            new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource1"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Controller Test String",
                        TestEnum = TestOptions.OptionE,
                        TestInteger = 50,
                    }
                ),
            },
            new EventEntity
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://example.com/TestSource2"),
                Type = "test.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Controller Test String",
                        TestEnum = TestOptions.OptionE,
                        TestInteger = 100,
                    }
                ),
            },
        };

        string serialized = JsonSerializer.Serialize(eventEntities);
        StringContent content = new(serialized, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage postResponse = await _client.PostAsync("/events", content);

        postResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage response = await _client.GetAsync("/Events");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("TestSource1");
        responseContent.ShouldContain("TestSource2");
    }
}

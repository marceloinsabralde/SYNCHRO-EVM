// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using Kumara.EventSource.Controllers;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public sealed class RoutingTests
{
    private readonly HttpClient _client;

    private readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();

    public RoutingTests()
    {
        WebApplicationFactory<Program> factory =
            new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddSingleton(_eventRepository))
            );
        _client = factory.CreateClient();
    }

    [TestMethod]
    public async Task GetEvents_EndpointIsActive()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/events");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task PostEvents_EndpointIsActive()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync(
            "/events",
            new StringContent("[]", System.Text.Encoding.UTF8, "application/cloudevents-batch+json")
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

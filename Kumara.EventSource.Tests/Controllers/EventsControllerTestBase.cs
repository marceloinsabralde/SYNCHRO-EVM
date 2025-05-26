// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Controllers;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.EventSource.Tests.Controllers;

public abstract class EventsControllerTestBase
{
    protected readonly HttpClient _client;
    protected readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();
    protected const string ApiBasePath = "/api/v1/events";

    protected EventsControllerTestBase()
    {
        WebApplicationFactory<EventsController> factory =
            new WebApplicationFactory<EventsController>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => services.AddSingleton(_eventRepository));
                builder.UseSetting("https_port", "7104");
            });

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost:7104"),
            }
        );
    }

    protected string GetEventsEndpoint(string? queryString = null)
    {
        return string.IsNullOrEmpty(queryString) ? ApiBasePath : $"{ApiBasePath}?{queryString}";
    }
}

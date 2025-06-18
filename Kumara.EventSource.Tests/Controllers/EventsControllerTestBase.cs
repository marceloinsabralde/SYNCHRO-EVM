// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Controllers;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Kumara.TestCommon.Helpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.EventSource.Tests.Controllers;

public abstract class EventsControllerTestBase : IDisposable
{
    private readonly AppServicesHelper.AppFactory _factory;
    protected readonly HttpClient _client;
    protected readonly IEventRepository _eventRepository = new EventRepositoryInMemoryList();
    protected readonly LinkGenerator _linkGenerator;
    protected static readonly JsonSerializerOptions JsonOptions = KumaraJsonOptions.DefaultOptions;

    protected EventsControllerTestBase()
    {
        _factory = AppServicesHelper.CreateWebApplicationFactory(builder =>
        {
            builder.ConfigureServices(services => services.AddSingleton(_eventRepository));
        });
        _client = _factory.CreateClient();
        _linkGenerator = _factory.GetRequiredService<LinkGenerator>();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    protected string GetEventsEndpoint(object? values = null)
    {
        return _linkGenerator.GetPathByName("GetEvents", values)!;
    }
}

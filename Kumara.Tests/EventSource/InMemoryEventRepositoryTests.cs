// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class InMemoryEventRepositoryTests
{
    private static IEventRepository CreateInMemoryRepository()
    {
        return new EventRepositoryInMemoryList();
    }

    [TestMethod]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveEventEntities()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();

        await eventRepository.AddEventsAsync(eventEntities);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.GetAllEventsAsync();

        eventEntities
            .Select(e => e.ITwinGuid)
            .ShouldBeSubsetOf(retrievedEvents.Select(e => e.ITwinGuid));
    }
}

// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
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

    [TestMethod]
    public async Task QueryEventsByITwinGuid_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereITwinGuid(targetITwinGuid);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.ITwinGuid == targetITwinGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByAccountGuid_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        Guid targetAccountGuid = eventEntities.First().AccountGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereAccountGuid(targetAccountGuid);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.AccountGuid == targetAccountGuid).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByCorrelationId_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string? targetCorrelationId = eventEntities.First().CorrelationId;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereCorrelationId(targetCorrelationId);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.CorrelationId == targetCorrelationId).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsByType_ShouldReturnMatchingEvents()
    {
        IEventRepository? eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string? targetType = eventEntities.First().Type;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereType(targetType);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents.All(e => e.Type == targetType).ShouldBeTrue();
    }

    [TestMethod]
    public async Task QueryEventsWithBuilder_ShouldReturnMatchingEvents()
    {
        IEventRepository eventRepository = CreateInMemoryRepository();
        List<EventEntity> eventEntities = EventRepositoryTestUtils.GetTestEventEntities();
        await eventRepository.AddEventsAsync(eventEntities);
        string targetType = eventEntities.First().Type;
        Guid targetITwinGuid = eventEntities.First().ITwinGuid;

        EventEntityQueryBuilder queryBuilder = new EventEntityQueryBuilder()
            .WhereType(targetType)
            .WhereITwinGuid(targetITwinGuid);
        IQueryable<EventEntity> retrievedEvents = await eventRepository.QueryEventsAsync(
            queryBuilder
        );

        retrievedEvents.ShouldNotBeEmpty();
        retrievedEvents
            .All(e => e.Type == targetType && e.ITwinGuid == targetITwinGuid)
            .ShouldBeTrue();
    }
}

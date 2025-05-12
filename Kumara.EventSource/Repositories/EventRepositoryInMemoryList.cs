// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Concurrent;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryInMemoryList : IEventRepository
{
    private readonly ConcurrentBag<EventEntity> _events = new();

    public Task<IQueryable<EventEntity>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.AsQueryable());
    }

    public Task AddEventsAsync(IEnumerable<EventEntity> events)
    {
        foreach (EventEntity eventEntity in events)
        {
            _events.Add(eventEntity);
        }

        return Task.CompletedTask;
    }

    public Task<IQueryable<EventEntity>> QueryEventsAsync(EventEntityQueryBuilder queryBuilder)
    {
        var result = queryBuilder.ApplyTo(_events.AsQueryable());
        return Task.FromResult(result);
    }
}

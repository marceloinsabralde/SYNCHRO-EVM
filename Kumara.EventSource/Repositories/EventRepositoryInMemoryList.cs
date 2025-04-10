// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Concurrent;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models; // Added EventEntity import

namespace Kumara.EventSource.Repositories;

public class EventRepositoryInMemoryList : IEventRepository
{
    private readonly ConcurrentBag<EventEntity> _events = new(); // Updated to use EventEntity

    public Task<IQueryable<EventEntity>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.AsQueryable());
    }

    public Task AddEventsAsync(IEnumerable<EventEntity> events)
    {
        foreach (var eventEntity in events)
        {
            _events.Add(eventEntity);
        }

        return Task.CompletedTask;
    }
}

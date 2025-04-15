// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Collections.Concurrent;
using CloudNative.CloudEvents;
using Kumara.EventSource.Interfaces;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryInMemoryList : IEventRepository
{
    private readonly ConcurrentBag<CloudEvent> _events = new();

    public Task<IQueryable<CloudEvent>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.AsQueryable());
    }

    public Task AddEventsAsync(IEnumerable<CloudEvent> cloudEvents)
    {
        foreach (CloudEvent cloudEvent in cloudEvents)
        {
            _events.Add(cloudEvent);
        }

        return Task.CompletedTask;
    }
}

// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Concurrent;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryInMemoryList : IEventRepository
{
    private readonly ConcurrentBag<Event> _events = [];

    public Task AddEventsAsync(
        IEnumerable<Event> events,
        CancellationToken cancellationToken = default
    )
    {
        foreach (Event? @event in events)
        {
            _events.Add(@event);
        }

        return Task.CompletedTask;
    }

    public Task<IQueryable<Event>> QueryEventsAsync(
        EventQueryBuilder queryBuilder,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Event>? result = queryBuilder.ApplyTo(
            (IQueryable<Event>)_events.AsQueryable().OrderBy(e => e.Id)
        );
        return Task.FromResult(result);
    }

    public async Task<PaginatedList<Event>> GetPaginatedEventsAsync(
        EventQueryBuilder queryBuilder,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        queryBuilder.Limit(pageSize + 1);
        IQueryable<Event> queryResult = await QueryEventsAsync(queryBuilder, cancellationToken);
        List<Event> events = queryResult.ToList();
        bool hasMoreItems = events.Count > pageSize;
        List<Event> pagedItems = hasMoreItems ? events.Take(pageSize).ToList() : events;

        PaginatedList<Event> result = new() { Items = pagedItems, HasMoreItems = hasMoreItems };

        return result;
    }
}

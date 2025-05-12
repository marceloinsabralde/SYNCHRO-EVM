// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Concurrent;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryInMemoryList : IEventRepository
{
    private readonly ConcurrentBag<EventEntity> _events = [];

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
        var result = queryBuilder.ApplyTo(
            (IQueryable<EventEntity>)_events.AsQueryable().OrderBy(e => e.Id)
        );
        return Task.FromResult(result);
    }

    public async Task<PaginatedList<EventEntity>> GetPaginatedEventsAsync(
        EventEntityQueryBuilder queryBuilder,
        int pageSize
    )
    {
        queryBuilder.Limit(pageSize + 1);
        IQueryable<EventEntity> queryResult = await QueryEventsAsync(queryBuilder);
        List<EventEntity> events = queryResult.ToList();
        bool hasMoreItems = events.Count > pageSize;
        List<EventEntity> pagedItems = hasMoreItems ? events.Take(pageSize).ToList() : events;

        PaginatedList<EventEntity> result = new()
        {
            Items = pagedItems,
            HasMoreItems = hasMoreItems,
        };

        return result;
    }
}

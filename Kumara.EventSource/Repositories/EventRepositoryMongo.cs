// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Extensions;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryMongo : IEventRepository
{
    private readonly MongoDbContext _context;

    public EventRepositoryMongo(MongoDbContext context)
    {
        _context = context;
    }

    public async Task AddEventsAsync(IEnumerable<EventEntity> events)
    {
        await _context.Events.AddRangeAsync(events);
        await _context.SaveChangesAsync();
    }

    public Task<IQueryable<EventEntity>> QueryEventsAsync(EventEntityQueryBuilder queryBuilder)
    {
        var result = queryBuilder.ApplyTo(_context.Events.AsQueryable().OrderBy(e => e.Id));
        return Task.FromResult(result);
    }

    public async Task<PaginatedList<EventEntity>> GetPaginatedEventsAsync(
        EventEntityQueryBuilder queryBuilder,
        int pageSize
    )
    {
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

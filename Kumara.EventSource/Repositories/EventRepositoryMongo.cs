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

    public async Task AddEventsAsync(IEnumerable<Event> events)
    {
        await _context.Events.AddRangeAsync(events);
        await _context.SaveChangesAsync();
    }

    public Task<IQueryable<Event>> QueryEventsAsync(EventQueryBuilder queryBuilder)
    {
        IQueryable<Event>? result = queryBuilder.ApplyTo(
            _context.Events.AsQueryable().OrderBy(e => e.Id)
        );
        return Task.FromResult(result);
    }

    public async Task<PaginatedList<Event>> GetPaginatedEventsAsync(
        EventQueryBuilder queryBuilder,
        int pageSize
    )
    {
        IQueryable<Event> queryResult = await QueryEventsAsync(queryBuilder);

        List<Event> events = queryResult.ToList();

        bool hasMoreItems = events.Count > pageSize;

        List<Event> pagedItems = hasMoreItems ? events.Take(pageSize).ToList() : events;

        PaginatedList<Event> result = new() { Items = pagedItems, HasMoreItems = hasMoreItems };

        return result;
    }
}

// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryMongo : IEventRepository
{
    private readonly MongoDbContext _context;

    public EventRepositoryMongo(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<EventEntity>> GetAllEventsAsync()
    {
        return await Task.FromResult(_context.Events.AsQueryable());
    }

    public async Task AddEventsAsync(IEnumerable<EventEntity> events)
    {
        await _context.Events.AddRangeAsync(events);
        await _context.SaveChangesAsync();
    }

    public Task<IQueryable<EventEntity>> QueryEventsAsync(EventEntityQueryBuilder queryBuilder)
    {
        var result = queryBuilder.ApplyTo(_context.Events.AsQueryable());
        return Task.FromResult(result);
    }
}

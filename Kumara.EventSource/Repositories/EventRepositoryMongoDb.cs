// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using CloudNative.CloudEvents;

using Kumara.EventSource.Interfaces;

using MongoDB.Driver;

namespace Kumara.EventSource.Repositories;

public class EventRepositoryMongoDb(IMongoDatabase database) : IEventRepository
{
    private readonly IMongoCollection<CloudEvent> _events = database.GetCollection<CloudEvent>("Events");

    public async Task<IQueryable<CloudEvent>> GetAllEventsAsync()
    {
        List<CloudEvent>? events = await _events.Find(e => true).ToListAsync();
        return events.AsQueryable();
    }

    public async Task AddEventsAsync(IEnumerable<CloudEvent> cloudEvents)
    {
        await _events.InsertManyAsync(cloudEvents);
    }
}

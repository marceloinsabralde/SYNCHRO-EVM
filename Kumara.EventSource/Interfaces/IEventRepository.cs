// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;

namespace Kumara.EventSource.Interfaces;

public interface IEventRepository
{
    Task<IQueryable<EventEntity>> GetAllEventsAsync();
    Task AddEventsAsync(IEnumerable<EventEntity> events);
    Task<IQueryable<EventEntity>> QueryEventsAsync(EventEntityQueryBuilder queryBuilder);
}

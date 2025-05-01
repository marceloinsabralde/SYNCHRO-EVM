// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;

namespace Kumara.EventSource.Interfaces;

public interface IEventRepository
{
    Task AddEventsAsync(IEnumerable<Event> events);
    Task<IQueryable<Event>> QueryEventsAsync(EventQueryBuilder queryBuilder);

    Task<PaginatedList<Event>> GetPaginatedEventsAsync(
        EventQueryBuilder queryBuilder,
        int pageSize
    );
}

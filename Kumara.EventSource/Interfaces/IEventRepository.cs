// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;

namespace Kumara.EventSource.Interfaces;

public interface IEventRepository
{
    Task AddEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default);

    Task<IQueryable<Event>> QueryEventsAsync(
        EventQueryBuilder queryBuilder,
        CancellationToken cancellationToken = default
    );

    Task<PaginatedList<Event>> GetPaginatedEventsAsync(
        EventQueryBuilder queryBuilder,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}

// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.EventSourceToo.Models;

namespace Kumara.EventSourceToo.Queries;

public class ListEventsQuery : PageableQuery<ListEventsQuery, ListEventsQueryFilter, Event>
{
    public ListEventsQuery(IQueryable<Event> query)
        : base(query)
    {
        _query = query.OrderBy(@event => @event.Id);
    }

    public override ListEventsQuery ApplyFilter(ListEventsQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(@event => @event.Id > filter.ContinueFromId);

        if (filter.ITwinId is not null)
            _query = _query.Where(@event => @event.ITwinId == filter.ITwinId);

        if (filter.AccountId is not null)
            _query = _query.Where(@event => @event.AccountId == filter.AccountId);

        if (filter.Type is not null)
            _query = _query.Where(@event => @event.Type == filter.Type);

        return this;
    }
}

public class ListEventsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }

    public Guid? ITwinId { get; set; }

    public Guid? AccountId { get; set; }

    public string? Type { get; set; }
}

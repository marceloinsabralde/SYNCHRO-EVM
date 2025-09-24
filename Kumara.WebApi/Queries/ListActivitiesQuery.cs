// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListActivitiesQuery
    : PageableQuery<ListActivitiesQuery, ListActivitiesQueryFilter, Activity>
{
    public ListActivitiesQuery(IQueryable<Activity> query, Guid iTwinId)
        : base(query)
    {
        _query = _query
            .Where(activity => activity.ITwinId == iTwinId)
            .OrderBy(activity => activity.Id);
    }

    public override ListActivitiesQuery ApplyFilter(ListActivitiesQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(activity => activity.Id > filter.ContinueFromId);

        if (filter.ControlAccountId is not null)
            _query = _query.Where(act => act.ControlAccountId == filter.ControlAccountId);

        return this;
    }
}

public record ListActivitiesQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
    public Guid? ControlAccountId { get; set; }
}

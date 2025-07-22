// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListActivitiesQuery
    : IPageableQuery<ListActivitiesQuery, ListActivitiesQueryFilter, Activity>
{
    private IQueryable<Activity> _query;
    private int _limit = 50;

    public ListActivitiesQuery(IQueryable<Activity> query, Guid iTwinId)
    {
        _query = query
            .Where(activity => activity.ITwinId == iTwinId)
            .OrderBy(activity => activity.Id);
    }

    public ListActivitiesQuery ApplyFilter(ListActivitiesQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(activity => activity.Id > filter.ContinueFromId);

        if (filter.ControlAccountId is not null)
            _query = _query.Where(act => act.ControlAccountId == filter.ControlAccountId);

        return this;
    }

    public ListActivitiesQuery WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<Activity> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<Activity>() { Items = items, HasMore = hasMore };
    }
}

public record ListActivitiesQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
    public Guid? ControlAccountId { get; set; }
}

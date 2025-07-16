// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Database;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListActivitiesQuery
{
    private IQueryable<Activity> _query;

    public ListActivitiesQuery(ApplicationDbContext dbContext, Guid iTwinId)
    {
        _query = dbContext
            .Activities.Where(activity => activity.ITwinId == iTwinId)
            .OrderBy(activity => activity.Id);
    }

    public ListActivitiesQuery ApplyFilter(QueryFilter filter)
    {
        if (filter.ControlAccountId is not null)
            _query = _query.Where(act => act.ControlAccountId == filter.ControlAccountId);

        return this;
    }

    public QueryResult<Activity> ExecuteQuery()
    {
        var items = _query.ToList();

        return new QueryResult<Activity>() { Items = items };
    }

    public record QueryResult<T>
    {
        public required List<T> Items;
    }

    public record QueryFilter
    {
        public Guid? ControlAccountId { get; set; }
    }
}

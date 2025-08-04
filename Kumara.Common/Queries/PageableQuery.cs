// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;

namespace Kumara.Common.Queries;

public abstract class PageableQuery<TQuery, TFilter, TPageableEntity>
    where TPageableEntity : IPageableEntity
    where TFilter : IPageableQueryFilter
{
    protected IQueryable<TPageableEntity> _query;
    private int _limit = 50;

    protected PageableQuery(IQueryable<TPageableEntity> query)
    {
        _query = query;
    }

    public abstract TQuery ApplyFilter(TFilter filter);

    public PageableQuery<TQuery, TFilter, TPageableEntity> WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<TPageableEntity> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<TPageableEntity>() { Items = items, HasMore = hasMore };
    }
}

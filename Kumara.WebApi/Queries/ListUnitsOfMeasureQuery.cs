// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Queries;

public class ListUnitsOfMeasureQuery
    : IPageableQuery<ListUnitsOfMeasureQuery, ListUnitsOfMeasureQueryFilter, UnitOfMeasure>
{
    private IQueryable<UnitOfMeasure> _query;
    private int _limit = 50;

    public ListUnitsOfMeasureQuery(IQueryable<UnitOfMeasure> query, Guid iTwinId)
    {
        _query = query.AsNoTracking().Where(uom => uom.ITwinId == iTwinId).OrderBy(uom => uom.Id);
    }

    public ListUnitsOfMeasureQuery ApplyFilter(ListUnitsOfMeasureQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(uom => uom.Id > filter.ContinueFromId);

        return this;
    }

    public ListUnitsOfMeasureQuery WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<UnitOfMeasure> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<UnitOfMeasure>() { Items = items, HasMore = hasMore };
    }
}

public class ListUnitsOfMeasureQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
